using Newtonsoft.Json;

namespace GaRyan2.PlutoTvAPI
{
    public class PlutoTimeline
    {
        [JsonProperty("_id")]
        public string ID { get; set; }

        [JsonProperty("start")]
        public string Start { get; set; }

        [JsonProperty("stop")]
        public string Stop { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("episode")]
        public PlutoProgramme Episode { get; set; }
    }
}