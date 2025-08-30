using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using YouTubeSubManager.Models;

namespace YouTubeSubManager.Services
{
    public static class CategoryStore
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static void Save(string path, CategoryBook book)
        {
            var json = JsonSerializer.Serialize(book, Options);
            File.WriteAllText(path, json);
        }

        public static CategoryBook Load(string path)
        {
            var json = File.ReadAllText(path);
            var book = JsonSerializer.Deserialize<CategoryBook>(json, Options);
            return book ?? new CategoryBook();
        }
    }
}
