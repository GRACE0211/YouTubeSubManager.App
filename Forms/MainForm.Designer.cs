namespace YouTubeSubManager;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        statusMain = new StatusStrip();
        lblCounts = new ToolStripStatusLabel();
        flowLayoutPanel1 = new FlowLayoutPanel();
        btnImport = new Button();
        btnAddCat = new Button();
        btnDelCat = new Button();
        btnSaveJson = new Button();
        btnLoadJson = new Button();
        btnSaveCatDb = new Button();
        btnLoadCatDb = new Button();
        btnExportSql = new Button();
        btnExportDb = new Button();
        splitMain = new SplitContainer();
        tableLayoutPanel1 = new TableLayoutPanel();
        txtSearch = new TextBox();
        lstAll = new ListBox();
        tvCategories = new TreeView();
        menuCategory = new ContextMenuStrip(components);
        miRename = new ToolStripMenuItem();
        miDelete = new ToolStripMenuItem();
        pnlLoading = new Panel();
        picLoading = new PictureBox();
        statusMain.SuspendLayout();
        flowLayoutPanel1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
        splitMain.Panel1.SuspendLayout();
        splitMain.Panel2.SuspendLayout();
        splitMain.SuspendLayout();
        tableLayoutPanel1.SuspendLayout();
        menuCategory.SuspendLayout();
        pnlLoading.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picLoading).BeginInit();
        SuspendLayout();
        // 
        // statusMain
        // 
        statusMain.BackColor = SystemColors.ActiveCaption;
        statusMain.Items.AddRange(new ToolStripItem[] { lblCounts });
        statusMain.Location = new Point(0, 653);
        statusMain.Name = "statusMain";
        statusMain.Size = new Size(1184, 22);
        statusMain.TabIndex = 0;
        statusMain.Text = "statusStrip1";
        // 
        // lblCounts
        // 
        lblCounts.Name = "lblCounts";
        lblCounts.Size = new Size(137, 17);
        lblCounts.Text = "總頻道數/已分類/未分類";
        // 
        // flowLayoutPanel1
        // 
        flowLayoutPanel1.BackColor = Color.LightSteelBlue;
        flowLayoutPanel1.BorderStyle = BorderStyle.FixedSingle;
        flowLayoutPanel1.Controls.Add(btnImport);
        flowLayoutPanel1.Controls.Add(btnAddCat);
        flowLayoutPanel1.Controls.Add(btnDelCat);
        flowLayoutPanel1.Controls.Add(btnSaveJson);
        flowLayoutPanel1.Controls.Add(btnLoadJson);
        flowLayoutPanel1.Controls.Add(btnSaveCatDb);
        flowLayoutPanel1.Controls.Add(btnLoadCatDb);
        flowLayoutPanel1.Controls.Add(btnExportSql);
        flowLayoutPanel1.Controls.Add(btnExportDb);
        flowLayoutPanel1.Dock = DockStyle.Bottom;
        flowLayoutPanel1.Location = new Point(0, 613);
        flowLayoutPanel1.Name = "flowLayoutPanel1";
        flowLayoutPanel1.Size = new Size(1184, 40);
        flowLayoutPanel1.TabIndex = 1;
        // 
        // btnImport
        // 
        btnImport.Font = new Font("微軟正黑體", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 136);
        btnImport.Location = new Point(3, 3);
        btnImport.Name = "btnImport";
        btnImport.Size = new Size(91, 33);
        btnImport.TabIndex = 0;
        btnImport.Text = "匯入訂閱";
        btnImport.UseVisualStyleBackColor = true;
        btnImport.Click += btnImport_Click;
        // 
        // btnAddCat
        // 
        btnAddCat.Font = new Font("微軟正黑體", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 136);
        btnAddCat.Location = new Point(100, 3);
        btnAddCat.Name = "btnAddCat";
        btnAddCat.Size = new Size(91, 33);
        btnAddCat.TabIndex = 1;
        btnAddCat.Text = "新增分類";
        btnAddCat.UseVisualStyleBackColor = true;
        btnAddCat.Click += btnAddCat_Click;
        // 
        // btnDelCat
        // 
        btnDelCat.Font = new Font("微軟正黑體", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 136);
        btnDelCat.Location = new Point(197, 3);
        btnDelCat.Name = "btnDelCat";
        btnDelCat.Size = new Size(91, 33);
        btnDelCat.TabIndex = 2;
        btnDelCat.Text = "刪除分類";
        btnDelCat.UseVisualStyleBackColor = true;
        btnDelCat.Click += btnDelCat_Click;
        // 
        // btnSaveJson
        // 
        btnSaveJson.Font = new Font("微軟正黑體", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 136);
        btnSaveJson.Location = new Point(294, 3);
        btnSaveJson.Name = "btnSaveJson";
        btnSaveJson.Size = new Size(115, 33);
        btnSaveJson.TabIndex = 3;
        btnSaveJson.Text = "儲存分類(JSON)";
        btnSaveJson.UseVisualStyleBackColor = true;
        btnSaveJson.Click += btnSaveJson_Click;
        // 
        // btnLoadJson
        // 
        btnLoadJson.Font = new Font("微軟正黑體", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 136);
        btnLoadJson.Location = new Point(415, 3);
        btnLoadJson.Name = "btnLoadJson";
        btnLoadJson.Size = new Size(115, 33);
        btnLoadJson.TabIndex = 4;
        btnLoadJson.Text = "載入分類(JSON)";
        btnLoadJson.UseVisualStyleBackColor = true;
        btnLoadJson.Click += btnLoadJson_Click;
        // 
        // btnSaveCatDb
        // 
        btnSaveCatDb.Font = new Font("微軟正黑體", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 136);
        btnSaveCatDb.Location = new Point(536, 3);
        btnSaveCatDb.Name = "btnSaveCatDb";
        btnSaveCatDb.Size = new Size(107, 33);
        btnSaveCatDb.TabIndex = 5;
        btnSaveCatDb.Text = "儲存分類到DB";
        btnSaveCatDb.UseVisualStyleBackColor = true;
        btnSaveCatDb.Click += btnSaveCatDb_Click;
        // 
        // btnLoadCatDb
        // 
        btnLoadCatDb.Font = new Font("微軟正黑體", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 136);
        btnLoadCatDb.Location = new Point(649, 3);
        btnLoadCatDb.Name = "btnLoadCatDb";
        btnLoadCatDb.Size = new Size(107, 33);
        btnLoadCatDb.TabIndex = 6;
        btnLoadCatDb.Text = "從DB載入分類";
        btnLoadCatDb.UseVisualStyleBackColor = true;
        btnLoadCatDb.Click += btnLoadCatDb_Click;
        // 
        // btnExportSql
        // 
        btnExportSql.Font = new Font("微軟正黑體", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 136);
        btnExportSql.Location = new Point(762, 3);
        btnExportSql.Name = "btnExportSql";
        btnExportSql.Size = new Size(86, 33);
        btnExportSql.TabIndex = 7;
        btnExportSql.Text = "匯出SQL";
        btnExportSql.UseVisualStyleBackColor = true;
        btnExportSql.Click += btnExportSql_Click;
        // 
        // btnExportDb
        // 
        btnExportDb.Font = new Font("微軟正黑體", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 136);
        btnExportDb.Location = new Point(854, 3);
        btnExportDb.Name = "btnExportDb";
        btnExportDb.Size = new Size(116, 33);
        btnExportDb.TabIndex = 8;
        btnExportDb.Text = "匯出SQLite DB";
        btnExportDb.UseVisualStyleBackColor = true;
        btnExportDb.Click += btnExportDb_Click;
        // 
        // splitMain
        // 
        splitMain.BackColor = Color.LightSteelBlue;
        splitMain.Dock = DockStyle.Fill;
        splitMain.FixedPanel = FixedPanel.Panel1;
        splitMain.Location = new Point(0, 0);
        splitMain.Name = "splitMain";
        // 
        // splitMain.Panel1
        // 
        splitMain.Panel1.Controls.Add(tableLayoutPanel1);
        // 
        // splitMain.Panel2
        // 
        splitMain.Panel2.Controls.Add(tvCategories);
        splitMain.Size = new Size(1184, 613);
        splitMain.SplitterDistance = 394;
        splitMain.TabIndex = 3;
        // 
        // tableLayoutPanel1
        // 
        tableLayoutPanel1.ColumnCount = 1;
        tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableLayoutPanel1.Controls.Add(txtSearch, 0, 0);
        tableLayoutPanel1.Controls.Add(lstAll, 0, 1);
        tableLayoutPanel1.Dock = DockStyle.Fill;
        tableLayoutPanel1.Location = new Point(0, 0);
        tableLayoutPanel1.Name = "tableLayoutPanel1";
        tableLayoutPanel1.RowCount = 2;
        tableLayoutPanel1.RowStyles.Add(new RowStyle());
        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableLayoutPanel1.Size = new Size(394, 613);
        tableLayoutPanel1.TabIndex = 0;
        // 
        // txtSearch
        // 
        txtSearch.Dock = DockStyle.Top;
        txtSearch.Location = new Point(3, 3);
        txtSearch.Name = "txtSearch";
        txtSearch.PlaceholderText = "搜尋頻道...";
        txtSearch.Size = new Size(388, 23);
        txtSearch.TabIndex = 0;
        txtSearch.TextChanged += txtSearch_TextChanged;
        // 
        // lstAll
        // 
        lstAll.BorderStyle = BorderStyle.FixedSingle;
        lstAll.Dock = DockStyle.Fill;
        lstAll.Font = new Font("微軟正黑體", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 136);
        lstAll.FormattingEnabled = true;
        lstAll.ItemHeight = 19;
        lstAll.Location = new Point(3, 32);
        lstAll.Name = "lstAll";
        lstAll.SelectionMode = SelectionMode.MultiExtended;
        lstAll.Size = new Size(388, 578);
        lstAll.TabIndex = 1;
        lstAll.DoubleClick += lstAll_DoubleClick;
        lstAll.MouseDown += lstAll_MouseDown;
        // 
        // tvCategories
        // 
        tvCategories.AllowDrop = true;
        tvCategories.BackColor = Color.AliceBlue;
        tvCategories.BorderStyle = BorderStyle.None;
        tvCategories.Dock = DockStyle.Fill;
        tvCategories.Font = new Font("微軟正黑體", 12F, FontStyle.Bold, GraphicsUnit.Point, 136);
        tvCategories.HideSelection = false;
        tvCategories.LabelEdit = true;
        tvCategories.Location = new Point(0, 0);
        tvCategories.Margin = new Padding(5);
        tvCategories.Name = "tvCategories";
        tvCategories.Size = new Size(786, 613);
        tvCategories.TabIndex = 0;
        tvCategories.NodeMouseClick += tvCategories_NodeMouseClick;
        tvCategories.NodeMouseDoubleClick += tvCategories_NodeMouseDoubleClick;
        tvCategories.DragDrop += tvCategories_DragDrop;
        tvCategories.DragEnter += tvCategories_DragEnterOrOver;
        tvCategories.DragOver += tvCategories_DragEnterOrOver;
        // 
        // menuCategory
        // 
        menuCategory.Items.AddRange(new ToolStripItem[] { miRename, miDelete });
        menuCategory.Name = "menuCategory";
        menuCategory.Size = new Size(123, 48);
        // 
        // miRename
        // 
        miRename.Name = "miRename";
        miRename.Size = new Size(122, 22);
        miRename.Text = "重新命名";
        miRename.Click += miRename_Click;
        // 
        // miDelete
        // 
        miDelete.Name = "miDelete";
        miDelete.Size = new Size(122, 22);
        miDelete.Text = "刪除分類";
        miDelete.Click += miDelete_Click;
        // 
        // pnlLoading
        // 
        pnlLoading.BackColor = Color.Black;
        pnlLoading.Controls.Add(picLoading);
        pnlLoading.Dock = DockStyle.Fill;
        pnlLoading.Location = new Point(0, 0);
        pnlLoading.Name = "pnlLoading";
        pnlLoading.Size = new Size(1184, 613);
        pnlLoading.TabIndex = 4;
        pnlLoading.Visible = false;
        // 
        // picLoading
        // 
        picLoading.BackColor = Color.Transparent;
        picLoading.Image = Resources.loadingGIF;
        picLoading.Location = new Point(412, 191);
        picLoading.Name = "picLoading";
        picLoading.Size = new Size(306, 187);
        picLoading.SizeMode = PictureBoxSizeMode.Zoom;
        picLoading.TabIndex = 0;
        picLoading.TabStop = false;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1184, 675);
        Controls.Add(pnlLoading);
        Controls.Add(splitMain);
        Controls.Add(flowLayoutPanel1);
        Controls.Add(statusMain);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Form1";
        statusMain.ResumeLayout(false);
        statusMain.PerformLayout();
        flowLayoutPanel1.ResumeLayout(false);
        splitMain.Panel1.ResumeLayout(false);
        splitMain.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
        splitMain.ResumeLayout(false);
        tableLayoutPanel1.ResumeLayout(false);
        tableLayoutPanel1.PerformLayout();
        menuCategory.ResumeLayout(false);
        pnlLoading.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)picLoading).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private StatusStrip statusMain;
    private FlowLayoutPanel flowLayoutPanel1;
    private SplitContainer splitMain;
    private TableLayoutPanel tableLayoutPanel1;
    private TextBox txtSearch;
    private ListBox lstAll;
    private TreeView tvCategories;
    private Button btnImport;
    private Button btnAddCat;
    private Button btnDelCat;
    private Button btnSaveJson;
    private Button btnLoadJson;
    private Button btnSaveCatDb;
    private Button btnLoadCatDb;
    private Button btnExportSql;
    private Button btnExportDb;
    private ToolStripStatusLabel lblCounts;
    private ContextMenuStrip menuCategory;
    private ToolStripMenuItem miRename;
    private ToolStripMenuItem miDelete;
    private Panel pnlLoading;
    private PictureBox picLoading;
}
