using Newtonsoft.Json;

namespace GaRyan2.PlutoTvAPI
{
    public class PlutoSeries
    {
        [JsonProperty("_id")]
        public string ID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("tile")]
        public PlutoImage Tile { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("featuredImage")]
        public PlutoImage FeaturedImage { get; set; }
    }
}