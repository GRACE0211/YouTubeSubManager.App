using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
{ get; set; }
這是 自動屬性 (Auto-Implemented Property)，代表：
get → 讀取值（拿出清單）
set → 設定值（整個換掉）
*/

namespace YouTubeSubManager.Models
{
    // 公開一個叫做 Categories 的屬性，它是一份 Category 的清單，
    // 可以讀寫，而且一開始就會自動準備好一個空的清單，避免是 null
    public class CategoryBook
    {
        // 屬性的名稱 -> book.Categories.Add(new Category { Name = "..." });
        public List<Category> Categories { get; set; } = new List<Category>();
    }

    public class Category
    {
        public string Name { get; set; } = string.Empty;
        public List<ChannelInfo> Channels { get; set; } = new List<ChannelInfo>();
    }
}