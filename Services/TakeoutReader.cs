using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using CsvHelper;
using YouTubeSubManager.Models;


/*  
    TakeoutReader.cs --
    判斷檔案副檔名是csv還是json檔，
    解析每一列，取出 ChannelId、Title、URL，
    組成一個一個 ChannelInfo 物件放到 List 裡
*/

namespace YouTubeSubManager.Services
{
    public static class TakeoutReader
    {
        // 1. 判斷檔案副檔名是csv還是json檔
        public static List<ChannelInfo> Read(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".csv" => ReadCsv(path),
                ".json" => ReadJson(path),
                _ => throw new NotSupportedException($"不支援的副檔名: {ext}")
            };
        }

        // 2-1. 如果是csv檔（有先安裝NuGet - CsvHelper）
        // Google Takeout 的 subscriptions.csv 常見欄位："Channel ID","Channel URL","Title"
        private static List<ChannelInfo> ReadCsv(string path)
        {
            using var reader = new StreamReader(path, true);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Read();
            csv.ReadHeader();

            string? Get(string name)
            {
                try { return csv.GetField(name); } catch { return null; }
            }

            var list = new List<ChannelInfo>();
            // 解析每一列，取出 ChannelId、Title、URL，組成一個一個 ChannelInfo 物件放到 List 裡
            while (csv.Read())
            {
                var id = Get("Channel ID") ?? Get("ChannelId") ?? Get("ChannelID") ?? Get("頻道 ID");
                var title = Get("Title") ?? Get("Channel Title") ?? Get("Name") ?? Get("頻道名稱");
                var url = Get("Channel URL") ?? Get("Url") ?? Get("Channel Url") ?? Get("頻道網址");

                // 如果 id 是空的，但 url 有值，就嘗試從 url 裡解析出 channelId
                if (string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(url))
                {
                    id = TryExtractChannelIdFromUrl(url);
                }

                // 如果最後有找到有效的 id
                if (!string.IsNullOrWhiteSpace(id))
                {
                    // 1. 先把 id 去掉多餘的空白，存到 safeId
                    string safeId = id.Trim();

                    // 2. 如果 title 沒填，就直接用 id 當作標題，否則用修剪過的 title
                    string safeTitle = string.IsNullOrWhiteSpace(title) ? safeId : title.Trim();

                    // 3. url 可能是空的，所以判斷一下：
                    //    - 如果 url 是 null 或空字串，就用 null
                    //    - 否則去掉多餘空白後存進 safeUrl
                    string? safeUrl = string.IsNullOrWhiteSpace(url) ? null : url.Trim();

                    // 4. 建立一個新的 ChannelInfo 物件，並加到 list 裡
                    list.Add(new ChannelInfo(safeId, safeTitle, safeUrl));
                }

            }
            // 最後回傳 List<ChannelInfo>
            return list
                // 如果有重複的id就分到同一組
                .GroupBy(c => c.ChannelId, StringComparer.OrdinalIgnoreCase)
                // 一個頻道 ID 只留第一筆，其它重複的丟掉
                .Select(g => g.First())
                // 結果依照頻道名稱（Title）排序，不分大小寫
                .OrderBy(c => c.Title, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        // string? 代表回傳值可能是一個字串，也可能是 null
        // 另一種寫法：private static string? TryExtractChannelIdFromUrl(string url)
        private static string TryExtractChannelIdFromUrl(string url)
        {
            // 常見格式：https://www.youtube.com/channel/UCxxxx
            // 去網址裡找「/channel/後面接一串字」的模式
            var m = Regex.Match(url, @"/channel/([A-Za-z0-9_-]+)");

            if (m.Success)
            {
                // 如果有找到，回傳頻道 ID
                return m.Groups[1].Value;
            }
            else
            {
                // 如果找不到，就回傳空字串（""）
                return string.Empty;
            }
        }


        // 2-2. 如果是JSON檔
        // Takeout 的 JSON 版本做寬鬆解析
        private static List<ChannelInfo> ReadJson(string path)
        {
            var json = File.ReadAllText(path);
            using var doc = JsonDocument.Parse(json);

            var list = new List<ChannelInfo>();

            // 找出裡面有的 channelId、title，一樣組成 ChannelInfo 丟進 List
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in doc.RootElement.EnumerateArray())
                    AddIfFound(el, list);
            }
            else if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (doc.RootElement.TryGetProperty("subscriptions", out var subs) && subs.ValueKind == JsonValueKind.Array)
                    foreach (var el in subs.EnumerateArray()) AddIfFound(el, list);
                else
                    AddIfFound(doc.RootElement, list);
            }

            return list
                .GroupBy(c => c.ChannelId, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(c => c.Title, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        // 幫助 ReadJson 從不同格式的 JSON 抓出 channelId、title
        private static void AddIfFound(JsonElement el, List<ChannelInfo> list)
        {
            // 代表 id/title 的型態可為null 或 string
            string? id = null;
            string? title = null;

            // 從 JSON 物件裡找 "channelId" 這個欄位，如果有就把它的值取出來當作 id (大小寫都找)
            if (el.TryGetProperty("channelId", out var cid)) id = cid.GetString();
            if (el.TryGetProperty("ChannelId", out var cid2)) id ??= cid2.GetString();

            // 抓 "title" 欄位，存到 title (大小寫都找)
            if (el.TryGetProperty("title", out var t)) title = t.GetString();
            if (el.TryGetProperty("Title", out var t2)) title ??= t2.GetString();

            // 萬一上面的方法都沒抓到，就深入 snippet 這個區塊去找 channelId 和 title
            if (id is null && el.TryGetProperty("snippet", out var snip))
            {
                if (snip.TryGetProperty("resourceId", out var rid) && rid.TryGetProperty("channelId", out var ridId))
                    id = ridId.GetString();
                if (title is null && snip.TryGetProperty("title", out var snipTitle))
                    title = snipTitle.GetString();
            }

            // 只要有抓到頻道 ID，就把它變成一個頻道物件丟進清單
            /*
            string.IsNullOrWhiteSpace(id) -> false, id不是空值 -> !取反就是true -> if(true) 進入迴圈
            string.IsNullOrWhiteSpace(id) -> true, id是空值 -> !取反就是false -> if(false) 不會進入迴圈
            if (id != null && id.Trim() != "")
            */
            if (!string.IsNullOrWhiteSpace(id))
            {
                // 先處理 id
                string safeId = id.Trim();

                // 如果 title 是空的，就用 id 當作 title
                string safeTitle = string.IsNullOrWhiteSpace(title) ? safeId : title.Trim();

                // 加到清單
                list.Add(new ChannelInfo(safeId, safeTitle, null));
            }

        }
    }
}