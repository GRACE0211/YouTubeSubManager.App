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

                if (string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(url))
                    id = TryExtractChannelIdFromUrl(url!);

                if (!string.IsNullOrWhiteSpace(id))
                    list.Add(new ChannelInfo(id!.Trim(), string.IsNullOrWhiteSpace(title) ? id! : title!.Trim(), url?.Trim()));
            }
            // 最後回傳 List<ChannelInfo>
            return list
                .GroupBy(c => c.ChannelId, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(c => c.Title, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        private static string? TryExtractChannelIdFromUrl(string url)
        {
            // 常見格式：https://www.youtube.com/channel/UCxxxx
            var m = Regex.Match(url, @"/channel/([A-Za-z0-9_-]+)");
            return m.Success ? m.Groups[1].Value : null;
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

            string? id = null;
            string? title = null;

            if (el.TryGetProperty("channelId", out var cid)) id = cid.GetString();
            if (el.TryGetProperty("ChannelId", out var cid2)) id ??= cid2.GetString();
            if (el.TryGetProperty("title", out var t)) title = t.GetString();
            if (el.TryGetProperty("Title", out var t2)) title ??= t2.GetString();

            if (id is null && el.TryGetProperty("snippet", out var snip))
            {
                if (snip.TryGetProperty("resourceId", out var rid) && rid.TryGetProperty("channelId", out var ridId))
                    id = ridId.GetString();
                if (title is null && snip.TryGetProperty("title", out var snipTitle))
                    title = snipTitle.GetString();
            }


            if (!string.IsNullOrWhiteSpace(id))
                list.Add(new ChannelInfo(id!.Trim(), string.IsNullOrWhiteSpace(title) ? id! : title!.Trim(), null));
        }
    }
}