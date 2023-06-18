using Newtonsoft.Json;

namespace GaRyan2.GithubApi
{
    public class Reaction
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("total_count")]
        public int TotalCount { get; set; }

        [JsonProperty("+1")]
        public int Plus1 { get; set; }

        [JsonProperty("-1")]
        public int Minus1 { get; set; }

        [JsonProperty("laugh")]
        public int Laugh { get; set; }

        [JsonProperty("hooray")]
        public int Hooray { get; set; }

        [JsonProperty("confused")]
        public int Confused { get; set; }

        [JsonProperty("heart")]
        public int Heart { get; set; }

        [JsonProperty("rocket")]
        public int Rocket { get; set; }

        [JsonProperty("eyes")]
        public int Eyes { get; set; }
    }
}