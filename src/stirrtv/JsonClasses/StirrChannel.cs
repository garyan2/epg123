using Newtonsoft.Json;

namespace GaRyan2.StirrTvApi
{
    public class StirrChannelStatus
    {
        [JsonProperty("rss")]
        public StirrRss ChannelRss { get; set; }
    }

    public class StirrRss
    {
        [JsonProperty("channel")]
        public StirrRssChannel Channel { get; set; }
    }

    public class StirrRssChannel
    {
        [JsonProperty("item")]
        public StirrRssChannelItem Item { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public class StirrRssChannelItem
    {
        [JsonProperty("link")]
        public string StreamUrl { get; set; }
    }
}