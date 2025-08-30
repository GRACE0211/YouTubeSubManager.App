using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace YouTubeSubManager.Models
{
    public class CategoryBook
    {
        public List<Category> Categories { get; set; } = new();
    }

    public class Category
    {
        public string Name { get; set; } = string.Empty;
        public List<ChannelInfo> Channels { get; set; } = new();
    }
}
