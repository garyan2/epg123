using Newtonsoft.Json;

namespace GaRyan2.StirrTvApi
{
    public class StirrLineup
    {
        [JsonProperty("channel")]
        public StirrChannel[] Channels { get; set; }

        [JsonProperty("categories")]
        public StirrCategory[] Categories { get; set; }
    }

    public class StirrChannel
    {
        [JsonProperty("icon")]
        public StirrIcon Icon { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("num")]
        public string ChannelNumber { get; set; }

        [JsonProperty("categories")]
        public StirrChannelCategory[] Categories { get; set; }
    }

    public class StirrIcon
    {
        [JsonProperty("src")]
        public string Source { get; set; }
    }

    public class StirrChannelCategory
    {
        [JsonProperty("uuid")]
        public string UUID { get; set; }
    }

    public class StirrCategory
    {
        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uuid")]
        public string UUID { get; set; }
    }
}