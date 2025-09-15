/*
    MainForm 匯入 CSV/JSON 後會建立很多個 ChannelInfo 物件，
    存進 ListBox 或 TreeView 來顯示
*/

namespace YouTubeSubManager.Models
{
    public class ChannelInfo
    {
        public string ChannelId { get; set; }
        public string Title { get; set; }
        public string? Url { get; set; }

        // 建構子（避免忘記給必填值）
        public ChannelInfo(string channelId, string title, string? url = null)
        {
            ChannelId = channelId;
            Title = title;
            Url = url;
        }

        /*
        因為 ListBox / TreeView 在顯示物件時，會自動呼叫 .ToString()
        如果沒有override，清單裡會出現一堆“YouTubeSubManager.Models.ChannelInfo”
        所以要改寫成回傳Title
        */
        public override string ToString()
        {
            return Title;
        }
    }
}