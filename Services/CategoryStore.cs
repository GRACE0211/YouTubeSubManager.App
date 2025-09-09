using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using YouTubeSubManager.Models;

/*
CategoryStore.cs --
專門處理「分類資料 (CategoryBook) 的存檔/讀檔」
把資料存成 JSON 檔，或從 JSON 檔讀回來的工具

JSON檔格式：
{
  "categories": [
    {
      "name": "音樂",
      "channels": [
        {
          "channelId": "UC456",
          "title": "周杰倫 Jay Chou",
          "url": "https://www.youtube.com/channel/UC456"
        }, 
        ...
      ]
    },
    ...
  ]
}

*/
namespace YouTubeSubManager.Services
{
    public static class CategoryStore
    {
        // 序列化設定，包含縮排、屬性質為null就不輸出、屬性名稱轉成小駝峰格式
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // 存成JSON檔
        // path 是要存檔的路徑，ex: C:\Users\Documents\categories.json
        public static void Save(string path, CategoryBook book)
        {
            var json = JsonSerializer.Serialize(book, Options);
            File.WriteAllText(path, json);
        }

        public static CategoryBook Load(string path)
        {
            // 把檔案的「原始文字內容」整份抓進來
            var json = File.ReadAllText(path);
            // 把剛才讀到的 JSON 字串 翻譯回 C# 物件，型別是 CategoryBook
            var book = JsonSerializer.Deserialize<CategoryBook>(json, Options);
            // '??' -> 如果左邊是 null，就用右邊的值
            return book ?? new CategoryBook();
        }
    }
}