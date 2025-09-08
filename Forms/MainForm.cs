using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YouTubeSubManager.Models;
using YouTubeSubManager.Services;
using System.Windows.Forms.VisualStyles;

namespace YouTubeSubManager
{
    public partial class MainForm : Form
    {
        // ====== 狀態資料 ======
        private readonly List<ChannelInfo> _allImported = new(); // 匯入到的全集
        private readonly List<ChannelInfo> _unassigned = new();  // 左側未分類清單
        private readonly BindingSource _binding = new();
        private readonly HashSet<string> _assignedIds = new(StringComparer.OrdinalIgnoreCase);
        private TreeNode? _dragHighlightNode = null; // 拖放時的高亮節點

        public MainForm()
        {
            InitializeComponent();
            // 如果沒有用設計器綁 Load 事件，可以在這裡手動：
            this.Load += MainForm_Load;


            // 建立 ImageList，先放三個基本圖示
            var imgs = new ImageList { ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(16, 16) };
            imgs.Images.Add("folder", Resources.folder);
            imgs.Images.Add("folder_open", Resources.folder_open);
            imgs.Images.Add("channel", Resources.channel);
            tvCategories.ImageList = imgs;
        }

        // ================= 初始綁定 =================
        private void MainForm_Load(object? sender, EventArgs e)
        {
            // ListBox 綁定
            _binding.DataSource = _unassigned;
            lstAll.DisplayMember = nameof(ChannelInfo.Title);
            lstAll.DataSource = _binding;

            // TreeView 建議屬性（你在設計器已設定過就不用）
            //tvCategories.HideSelection = false;
            //tvCategories.LabelEdit = true;
            //tvCategories.AllowDrop = true;

            UpdateCounts();

            lstAll.DrawMode = DrawMode.OwnerDrawFixed; // 自訂繪製
            lstAll.ItemHeight = 28;            // 行高
            lstAll.BorderStyle = BorderStyle.FixedSingle; // 外框（可換成 None）

            lstAll.DrawItem += (s, e) =>
            {
                e.DrawBackground();

                // 交錯色（可選）
                if ((e.Index % 2) == 0 && (e.State & DrawItemState.Selected) == 0)
                    e.Graphics.FillRectangle(Brushes.WhiteSmoke, e.Bounds);

                // 文字
                if (e.Index >= 0)
                {
                    var text = (lstAll.Items[e.Index] as YouTubeSubManager.Models.ChannelInfo)?.Title ?? lstAll.Items[e.Index]?.ToString() ?? "";
                    TextRenderer.DrawText(e.Graphics, text, e.Font,
                        new Rectangle(e.Bounds.X + 8, e.Bounds.Y + 6, e.Bounds.Width - 16, e.Bounds.Height - 6),
                        ((e.State & DrawItemState.Selected) != 0) ? SystemColors.HighlightText : e.ForeColor,
                        TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                }

                // 每列底線
                using var pen = new Pen(Color.Gainsboro);
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);

                e.DrawFocusRectangle();
            };


            tvCategories.DrawMode = TreeViewDrawMode.OwnerDrawText;

            tvCategories.DrawNode += (s, e) =>
            {
                var g = e.Graphics;
                bool selected = (e.State & TreeNodeStates.Selected) != 0;

                // 背景
                var bg = selected ? SystemColors.Highlight
                      : ((e.Node.Index % 2 == 0) ? Color.FromArgb(216, 228, 249) : Color.White);
                using (var b = new SolidBrush(bg))
                    g.FillRectangle(b, new Rectangle(e.Bounds.X, e.Bounds.Y,  // ← 這裡原本是 0
                                                     tvCategories.Width - e.Bounds.X,
                                                     e.Bounds.Height));

                // 圖示（你原本的 A/B 任一法都可）
                if (tvCategories.ImageList != null)
                {
                    var key = selected && !string.IsNullOrEmpty(e.Node.SelectedImageKey)
                                ? e.Node.SelectedImageKey
                                : e.Node.ImageKey;
                    if (!string.IsNullOrEmpty(key) && tvCategories.ImageList.Images.ContainsKey(key))
                    {
                        var img = tvCategories.ImageList.Images[key];
                        var sz = tvCategories.ImageList.ImageSize;
                        int x = e.Bounds.Left - sz.Width - 3;              // 放在文字左邊
                        int y = e.Bounds.Top + (e.Bounds.Height - sz.Height) / 2;
                        g.DrawImage(img, new Rectangle(x, y, sz.Width, sz.Height));
                    }
                }

                // 文字
                var fore = selected ? SystemColors.HighlightText : e.Node.ForeColor;
                TextRenderer.DrawText(g, e.Node.Text, tvCategories.Font,
                    new Rectangle(e.Bounds.X, e.Bounds.Y, tvCategories.Width - e.Bounds.X, e.Bounds.Height),
                    fore, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                // 讓系統照常畫箭頭/線等預設項目
                e.DrawDefault = true;
                tvCategories.ShowPlusMinus = true;
            };


            tvCategories.HideSelection = false;


        }
        private void ApplyZebraColors()
        {
            int i = 0;
            foreach (TreeNode n in tvCategories.Nodes) Walk(n, ref i);
            void Walk(TreeNode node, ref int idx)
            {
                node.BackColor = (idx++ % 2 == 0) ? Color.FromArgb(246, 246, 246) : Color.White;
                foreach (TreeNode c in node.Nodes) Walk(c, ref idx);
            }
        }

        // ================= 搜尋 =================
        private void txtSearch_TextChanged(object sender, EventArgs e) => ApplyFilter();

        private void ApplyFilter()
        {
            var q = (txtSearch.Text ?? string.Empty).Trim();
            IEnumerable<ChannelInfo> data = _unassigned;
            if (!string.IsNullOrEmpty(q))
                data = data.Where(c => c.Title.Contains(q, StringComparison.CurrentCultureIgnoreCase));

            _binding.DataSource = data.ToList();
            lstAll.DataSource = _binding; // refresh
        }

        // ================= 匯入 CSV/JSON =================
        private async void btnImport_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "選擇 Google Takeout 訂閱檔 (CSV/JSON)",
                Filter = "CSV/JSON|*.csv;*.json|CSV|*.csv|JSON|*.json"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var list = await System.Threading.Tasks.Task.Run(() => TakeoutReader.Read(ofd.FileName));
                _allImported.Clear();
                _allImported.AddRange(list);

                // 重置 UI 狀態
                _assignedIds.Clear();
                _unassigned.Clear();
                _unassigned.AddRange(_allImported);
                tvCategories.Nodes.Clear();

                ApplyFilter();
                UpdateCounts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"匯入失敗：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================= 拖放：ListBox → TreeView =================
        private void lstAll_MouseDown(object sender, MouseEventArgs e)
        {
            if (lstAll.SelectedItems.Count == 0) return;
            var dragged = lstAll.SelectedItems.Cast<ChannelInfo>().ToList();
            if (dragged.Count > 0)
                lstAll.DoDragDrop(dragged, DragDropEffects.Move);
            //ApplyZebraColors();
        }

        private void tvCategories_DragEnterOrOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            if (!e.Data!.GetDataPresent(typeof(List<ChannelInfo>))) return;

            var pt = tvCategories.PointToClient(new Point(e.X, e.Y));
            var node = tvCategories.GetNodeAt(pt);

            if(_dragHighlightNode != node)
            {
                _dragHighlightNode = node;
                tvCategories.Invalidate();
            }

            if (IsCategoryNode(node))
                e.Effect = DragDropEffects.Move;
            //ApplyZebraColors();
        }

        private void tvCategories_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data!.GetDataPresent(typeof(List<ChannelInfo>))) return;
            var items = (List<ChannelInfo>)e.Data.GetData(typeof(List<ChannelInfo>))!;

            var pt = tvCategories.PointToClient(new Point(e.X, e.Y));
            var node = tvCategories.GetNodeAt(pt);
            if (!IsCategoryNode(node)) return;

            var cat = (Category)node!.Tag!;

            tvCategories.BeginUpdate();
            try
            {
                foreach (var ch in items)
                {
                    if (_assignedIds.Add(ch.ChannelId))
                    {
                        // 新增頻道子節點時
                        cat.Channels.Add(ch);
                        var child = node.Nodes.Add(ch.Title);
                        child.Tag = ch;
                        child.ImageKey = child.SelectedImageKey = "channel";

                        var idx = _unassigned.FindIndex(x => x.ChannelId.Equals(ch.ChannelId, StringComparison.OrdinalIgnoreCase));
                        if (idx >= 0) _unassigned.RemoveAt(idx);
                    }
                }
            }
            finally
            {
                tvCategories.EndUpdate();
            }

            ApplyFilter();
            UpdateCounts();
            ApplyZebraColors();
            _dragHighlightNode = node;
            tvCategories.Invalidate();
        }

        private void tvCategories_DragLeave(object sender, EventArgs e)
        {
                _dragHighlightNode = null;
                tvCategories.Invalidate();
        }

        private static bool IsCategoryNode(TreeNode? n) => n?.Tag is Category;
        private readonly ImageList _imgs = new() { ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(16, 16) };

        // ================= 分類：新增/刪除/重新命名 =================
        private void btnAddCat_Click(object sender, EventArgs e)
        {
            var name = NextCategoryName();
            var cat = new Category { Name = name };
            //var n = new TreeNode(name) { Tag = cat };
            var n = new TreeNode(name) { Tag = cat, ImageKey = "folder", SelectedImageKey = "folder_open" };
            tvCategories.Nodes.Add(n);
            tvCategories.SelectedNode = n;
            n.Expand();

            ApplyZebraColors();

        }

        private string NextCategoryName()
        {
            var baseName = "新分類";
            var i = 1;
            var exists = new HashSet<string>(tvCategories.Nodes.Cast<TreeNode>().Select(n => n.Text));
            while (exists.Contains($"{baseName} {i}")) i++;
            return $"{baseName} {i}";
        }

        private void btnDelCat_Click(object sender, EventArgs e)
        {
            var n = tvCategories.SelectedNode;
            if (!IsCategoryNode(n) || n is null) return;

            if (MessageBox.Show($"刪除分類『{n.Text}』？（子頻道將回到未分類）", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                return;

            foreach (TreeNode child in n.Nodes)
            {
                if (child.Tag is ChannelInfo ch)
                {
                    _assignedIds.Remove(ch.ChannelId);
                    if (_unassigned.All(x => !x.ChannelId.Equals(ch.ChannelId, StringComparison.OrdinalIgnoreCase)))
                        _unassigned.Add(ch);
                }
            }

            tvCategories.Nodes.Remove(n);
            ApplyFilter();
            UpdateCounts();
        }

        // 右鍵選單：只在分類節點顯示
        private void tvCategories_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                tvCategories.SelectedNode = e.Node;
                if (IsCategoryNode(e.Node))
                    menuCategory.Show(Cursor.Position);
            }
        }

        private void miRename_Click(object sender, EventArgs e)
        {
            tvCategories.SelectedNode?.BeginEdit();
        }

        private void miDelete_Click(object sender, EventArgs e)
        {
            // 共用刪除邏輯
            btnDelCat_Click(sender, e);
        }

        // ================= 開啟頻道（雙擊 ListBox / TreeView 子節點） =================
        private void lstAll_DoubleClick(object sender, EventArgs e)
        {
            if (lstAll.SelectedItem is ChannelInfo ch)
                OpenChannel(ch);
        }

        private void tvCategories_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node?.Tag is ChannelInfo ch)
                OpenChannel(ch);
        }

        private static void OpenChannel(ChannelInfo ch)
        {
            var url = !string.IsNullOrWhiteSpace(ch.Url)
                ? ch.Url!
                : $"https://www.youtube.com/channel/{ch.ChannelId}";
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"開啟瀏覽器失敗：{ex.Message}");
            }
        }

        // ================= JSON 儲存/載入（可選） =================
        private void btnSaveJson_Click(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog { Title = "儲存 categories.json", Filter = "JSON|*.json", FileName = "categories.json" };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            var book = BuildCategoryBookFromTree();
            try
            {
                CategoryStore.Save(sfd.FileName, book);
                MessageBox.Show("已儲存。", "訊息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"儲存失敗：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoadJson_Click(object sender, EventArgs e)
        {
            ApplyZebraColors();
            using var ofd = new OpenFileDialog { Title = "載入 categories.json", Filter = "JSON|*.json" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var book = CategoryStore.Load(ofd.FileName);

                tvCategories.BeginUpdate();
                tvCategories.Nodes.Clear();
                _assignedIds.Clear();

                foreach (var cat in book.Categories)
                {
                    var n = new TreeNode(cat.Name) { Tag = new Category { Name = cat.Name } };
                    tvCategories.Nodes.Add(n);
                    foreach (var ch in cat.Channels)
                    {
                        if (_assignedIds.Add(ch.ChannelId))
                        {
                            (n.Tag as Category)!.Channels.Add(ch);
                            var cnode = n.Nodes.Add(ch.Title);
                            cnode.Tag = ch;
                        }
                    }
                }

                tvCategories.EndUpdate();

                _unassigned.RemoveAll(x => _assignedIds.Contains(x.ChannelId));
                ApplyFilter();
                UpdateCounts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"載入失敗：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        private CategoryBook BuildCategoryBookFromTree()
        {
            var book = new CategoryBook();
            foreach (TreeNode n in tvCategories.Nodes)
            {
                if (!IsCategoryNode(n)) continue;
                var cat = new Category { Name = n.Text };
                foreach (TreeNode child in n.Nodes)
                {
                    if (child.Tag is ChannelInfo ch)
                        cat.Channels.Add(ch);
                }
                book.Categories.Add(cat);
            }
            return book;
        }

        // ================= 匯出 SQL（MySQL 腳本） =================
        private void btnExportSql_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "選擇 訂閱內容.csv",
                Filter = "CSV|*.csv"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            using var sfd = new SaveFileDialog
            {
                Title = "輸出 SQL 檔",
                Filter = "SQL|*.sql",
                FileName = "channels.sql"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                // 讀 CSV
                var rows = ReadCsvBasic(ofd.FileName);

                using var sw = new StreamWriter(sfd.FileName, false, new UTF8Encoding(false));
                sw.WriteLine("""
CREATE TABLE IF NOT EXISTS `channels` (
  `id` VARCHAR(64) PRIMARY KEY,
  `url` TEXT,
  `title` TEXT
) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

START TRANSACTION;
""");

                const int chunk = 500;
                for (int i = 0; i < rows.Count; i += chunk)
                {
                    var part = rows.Skip(i).Take(chunk);
                    sw.WriteLine("INSERT INTO `channels` (id, url, title) VALUES");
                    sw.WriteLine(string.Join(",\n", part.Select(r =>
                        $"('{SqlEscape(r.id)}','{SqlEscape(r.url)}','{SqlEscape(r.title)}')")) + ";");
                }
                sw.WriteLine("COMMIT;");
                sw.Flush();

                MessageBox.Show($"已輸出 {rows.Count} 筆到：\n{sfd.FileName}", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("匯出失敗：" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================= 匯出 SQLite DB（建立 channels.db） =================
        private void btnExportDb_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "選擇 訂閱內容.csv",
                Filter = "CSV|*.csv"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            using var sfd = new SaveFileDialog
            {
                Title = "輸出 SQLite DB 檔",
                Filter = "SQLite DB|*.db;*.sqlite",
                FileName = "channels.db"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                // 讀 CSV
                var rows = ReadCsvBasic(ofd.FileName);

                // 覆蓋舊檔
                if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);

                using var conn = new SqliteConnection($"Data Source={sfd.FileName}");
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS channels (id TEXT PRIMARY KEY, url TEXT, title TEXT);";
                    cmd.ExecuteNonQuery();
                }

                using (var tx = conn.BeginTransaction())
                using (var insert = conn.CreateCommand())
                {
                    insert.CommandText = "INSERT OR IGNORE INTO channels (id, url, title) VALUES ($id,$url,$title);";
                    var pId = insert.CreateParameter(); pId.ParameterName = "$id"; insert.Parameters.Add(pId);
                    var pUrl = insert.CreateParameter(); pUrl.ParameterName = "$url"; insert.Parameters.Add(pUrl);
                    var pTit = insert.CreateParameter(); pTit.ParameterName = "$title"; insert.Parameters.Add(pTit);

                    foreach (var (id, url, title) in rows)
                    {
                        pId.Value = id; pUrl.Value = url; pTit.Value = title;
                        insert.ExecuteNonQuery();
                    }
                    tx.Commit();
                }

                conn.Close();
                MessageBox.Show($"已輸出 {rows.Count} 筆到：\n{sfd.FileName}", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("匯出失敗：" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 讀 CSV 的共用小工具（回傳去重後的 (id,url,title)）
        private List<(string id, string url, string title)> ReadCsvBasic(string path)
        {
            var rows = new List<(string id, string url, string title)>();
            using var reader = new StreamReader(path, true);
            using var csv = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);
            csv.Read();
            csv.ReadHeader();
            var header = csv.HeaderRecord ?? Array.Empty<string>();

            int idxId = FindColumnIndex(header, new[] { "頻道 ID", "Channel ID", "ChannelId", "ChannelID" }, 0);
            int idxUrl = FindColumnIndex(header, new[] { "頻道網址", "Channel URL", "Url" }, 1);
            int idxTitle = FindColumnIndex(header, new[] { "頻道名稱", "Title", "Channel Title", "Name" }, 2);

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while (csv.Read())
            {
                string Get(int i)
                {
                    var rec = csv.Parser.Record ?? Array.Empty<string>();
                    return (i >= 0 && i < rec.Length ? (rec[i] ?? string.Empty) : string.Empty).Trim();
                }
                var id = Get(idxId);
                var url = Get(idxUrl);
                var title = Get(idxTitle);

                if (string.IsNullOrWhiteSpace(id)) continue;
                if (!seen.Add(id)) continue;
                rows.Add((id, url, string.IsNullOrWhiteSpace(title) ? id : title));
            }
            return rows;
        }

        private static int FindColumnIndex(string[] header, string[] candidates, int fallback)
        {
            var norm = header.Select(h => h.Trim().ToLowerInvariant()).ToArray();
            foreach (var c in candidates)
            {
                var key = c.Trim().ToLowerInvariant();
                for (int i = 0; i < norm.Length; i++)
                    if (norm[i] == key) return i;
            }
            return Math.Min(fallback, Math.Max(0, header.Length - 1));
        }

        private static string SqlEscape(string s) => (s ?? string.Empty).Replace("'", "''");

        // ================= 分類 ⇄ SQLite DB（把分類也存到 DB） =================
        private void btnSaveCatDb_Click(object sender, EventArgs e)
        {
            var path = PromptPickDb(forSave: true);
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                using var conn = new SqliteConnection($"Data Source={path}");
                conn.Open();

                // 建表
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS channels (id TEXT PRIMARY KEY, url TEXT, title TEXT);"; cmd.ExecuteNonQuery();
                    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS categories (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT UNIQUE);"; cmd.ExecuteNonQuery();
                    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS category_channels (category_id INTEGER, channel_id TEXT, PRIMARY KEY(category_id, channel_id));"; cmd.ExecuteNonQuery();
                }

                var book = BuildCategoryBookFromTree();

                using var tx = conn.BeginTransaction();
                using (var clear = conn.CreateCommand())
                {
                    clear.CommandText = "DELETE FROM category_channels; DELETE FROM categories;";
                    clear.ExecuteNonQuery();
                }

                foreach (var cat in book.Categories)
                {
                    long catId;
                    using (var insCat = conn.CreateCommand())
                    {
                        insCat.CommandText = "INSERT INTO categories(name) VALUES ($n); SELECT last_insert_rowid();";
                        insCat.Parameters.AddWithValue("$n", cat.Name);
                        catId = (long)(insCat.ExecuteScalar() ?? 0L);
                    }

                    using (var insMap = conn.CreateCommand())
                    {
                        insMap.CommandText = "INSERT OR IGNORE INTO category_channels(category_id, channel_id) VALUES ($cid,$chid);";
                        var pC = insMap.CreateParameter(); pC.ParameterName = "$cid"; insMap.Parameters.Add(pC);
                        var pH = insMap.CreateParameter(); pH.ParameterName = "$chid"; insMap.Parameters.Add(pH);
                        pC.Value = catId;

                        foreach (var ch in cat.Channels)
                        {
                            pH.Value = ch.ChannelId;
                            insMap.ExecuteNonQuery();
                        }
                    }
                }
                tx.Commit();

                MessageBox.Show("分類已儲存到 DB。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("儲存到 DB 失敗：" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoadCatDb_Click(object sender, EventArgs e)
        {
            ApplyZebraColors();
            var path = PromptPickDb(forSave: false);
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                using var conn = new SqliteConnection($"Data Source={path}");
                conn.Open();

                // 讀 categories
                var catList = new List<(long id, string name)>();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT id, name FROM categories ORDER BY name;";
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                        catList.Add((r.GetInt64(0), r.GetString(1)));
                }

                tvCategories.BeginUpdate();
                tvCategories.Nodes.Clear();
                _assignedIds.Clear();

                foreach (var (cid, name) in catList)
                {
                    var n = new TreeNode(name) { Tag = new Category { Name = name } };
                    tvCategories.Nodes.Add(n);

                    using var cmd = conn.CreateCommand();
                    cmd.CommandText =
                        "SELECT cc.channel_id, IFNULL(ch.title, cc.channel_id) AS title, ch.url " +
                        "FROM category_channels cc " +
                        "LEFT JOIN channels ch ON ch.id=cc.channel_id " +
                        "WHERE cc.category_id=$id ORDER BY title;";
                    cmd.Parameters.AddWithValue("$id", cid);

                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        var chid = r.GetString(0);
                        var title = r.GetString(1);
                        var url = r.IsDBNull(2) ? null : r.GetString(2);
                        var ch = new ChannelInfo(chid, title, url);

                        if (_assignedIds.Add(ch.ChannelId))
                        {
                            (n.Tag as Category)!.Channels.Add(ch);
                            var cnode = n.Nodes.Add(ch.Title);
                            cnode.Tag = ch;
                        }
                    }
                }

                tvCategories.EndUpdate();

                _unassigned.RemoveAll(x => _assignedIds.Contains(x.ChannelId));
                ApplyFilter();
                UpdateCounts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("從 DB 載入分類失敗：" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string? PromptPickDb(bool forSave)
        {
            if (forSave)
            {
                using var sfd = new SaveFileDialog { Title = "選擇/建立 SQLite DB", Filter = "SQLite DB|*.db;*.sqlite", FileName = "channels.db" };
                return sfd.ShowDialog() == DialogResult.OK ? sfd.FileName : null;
            }
            else
            {
                using var ofd = new OpenFileDialog { Title = "選擇 SQLite DB", Filter = "SQLite DB|*.db;*.sqlite" };
                return ofd.ShowDialog() == DialogResult.OK ? ofd.FileName : null;
            }
        }

        // ================= 計數 =================
        private void UpdateCounts()
        {
            var assigned = _assignedIds.Count;
            var total = assigned + _unassigned.Count;
            lblCounts.Text = $"總頻道數：{total}／已分類：{assigned}／未分類：{_unassigned.Count}";
        }
    }
}
