namespace YouTubeSubManager.Models
{
    public class ChannelInfo
    {
        public string ChannelId { get; set; }
        public string Title { get; set; }
        public string? Url { get; set; }

        // 建構子
        public ChannelInfo(string channelId, string title, string? url = null)
        {
            ChannelId = channelId.Trim();
            Title = string.IsNullOrWhiteSpace(title) ? channelId : title.Trim();
            Url = url?.Trim();
        }

        // 顯示時只顯示 Title
        public override string ToString()
        {
            return Title;
        }

        // 用 ChannelId 來判斷相等
        public override bool Equals(object? obj)
        {
            if (obj is ChannelInfo other)
            {
                return string.Equals(ChannelId, other.ChannelId, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ChannelId?.ToLowerInvariant().GetHashCode() ?? 0;
        }
    }
}
