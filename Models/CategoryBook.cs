using System.Collections.Generic;

namespace YouTubeSubManager.Models
{
    public class CategoryBook
    {
        public List<Category> Categories { get; set; }

        public CategoryBook()
        {
            Categories = new List<Category>();
        }
    }

    public class Category
    {
        public string Name { get; set; }
        public List<ChannelInfo> Channels { get; set; }

        public Category()
        {
            Name = string.Empty;
            Channels = new List<ChannelInfo>();
        }
    }
}
