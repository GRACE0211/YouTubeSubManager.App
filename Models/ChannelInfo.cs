

namespace YouTubeSubManager.Models
{
    public record ChannelInfo(string ChannelId, string Title, string? Url = null)
    {
        public override string ToString() => Title;
    }
}