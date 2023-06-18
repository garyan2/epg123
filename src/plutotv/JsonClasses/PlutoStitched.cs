using Newtonsoft.Json;
using System.Collections.Generic;

namespace GaRyan2.PlutoTvAPI
{
    public class PlutoStitched
    {
        [JsonProperty("urls")]
        public List<PlutoStitchedUrl> Urls { get; set; }

        [JsonProperty("sessionURL")]
        public string SessionURL { get; set; }
    }

    public class PlutoStitchedUrl
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}