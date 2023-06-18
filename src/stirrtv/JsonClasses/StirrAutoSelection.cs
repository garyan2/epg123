using Newtonsoft.Json;

namespace GaRyan2.StirrTvApi
{
    public class StirrAutoSelect
    {
        [JsonProperty("page")]
        public StirrAutoSelectPage[] Page { get; set; }
    }

    public class StirrAutoSelectPage
    {
        [JsonProperty("button")]
        public StirrMedia Button { get; set; }
    }

    public class StirrMedia
    {
        [JsonProperty("media:content")]
        public StirrMediaContent MediaContent { get; set; }
    }

    public class StirrMediaContent
    {
        [JsonProperty("sinclair:action_config")]
        public StirrActionConfig Config { get; set; }
    }

    public class StirrActionConfig
    {
        [JsonProperty("station")]
        public string[] Stations { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }
    }
}