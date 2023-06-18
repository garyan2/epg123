using Newtonsoft.Json;

namespace GaRyan2.PlutoTvAPI
{
    public class PlutoProgramme
    {
        [JsonProperty("_id")]
        public string ID { get; set; }

        [JsonProperty("number")]
        public int Number { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("genre")]
        public string Genre { get; set; }

        [JsonProperty("subGenre")]
        public string SubGenre { get; set; }

        [JsonProperty("distributeAs")]
        public PlutoDistributeAs DistributeAs { get; set; }

        [JsonProperty("clip")]
        public PlutoClip Clip { get; set; }

        [JsonProperty("rating")]
        public string Rating { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("poster")]
        public PlutoImage Poster { get; set; }

        [JsonProperty("firstAired")]
        public string FirstAired { get; set; }

        [JsonProperty("thumbnail")]
        public PlutoImage Thumbnail { get; set; }

        [JsonProperty("liveBroadcast")]
        public bool LiveBroadcast { get; set; }

        [JsonProperty("featuredImage")]
        public PlutoImage FeaturedImage { get; set; }

        [JsonProperty("series")]
        public PlutoSeries Series { get; set; }
    }
}