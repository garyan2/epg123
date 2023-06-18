using Newtonsoft.Json;
using System.Collections.Generic;

namespace GaRyan2.PlutoTvAPI
{
    public class PlutoChannel
    {
        public override string ToString()
        {
            return string.Format("{0} {1}", Number, Name);
        }

        [JsonProperty("_id")]
        public string ID { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("number")]
        public int Number { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("visibility")]
        public string Visibility { get; set; }

        [JsonProperty("onDemandDescription")]
        public string OnDemandDescription { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("plutoOfficeOnly")]
        public bool PlutoOfficeOnly { get; set; }

        [JsonProperty("isStitched")]
        public bool IsStitched { get; set; }

        [JsonProperty("directOnly")]
        public bool DirectOnly { get; set; }

        [JsonProperty("chatRoomId")]
        public int ChatRoomId { get; set; }

        [JsonProperty("chatEnabled")]
        public bool ChatEnabled { get; set; }

        [JsonProperty("onDemand")]
        public bool OnDemand { get; set; }

        [JsonProperty("cohortMask")]
        public int CohortMask { get; set; }

        [JsonProperty("featuredImage")]
        public PlutoImage FeaturedImage { get; set; }

        [JsonProperty("thumbnail")]
        public PlutoImage Thumbnail { get; set; }

        [JsonProperty("tile")]
        public PlutoImage Tile { get; set; }

        [JsonProperty("logo")]
        public PlutoImage Logo { get; set; }

        [JsonProperty("colorLogoSVG")]
        public PlutoImage ColorLogoSVG { get; set; }

        [JsonProperty("colorLogoPNG")]
        public PlutoImage ColorLogoPNG { get; set; }

        [JsonProperty("solidLogoSVG")]
        public PlutoImage SolidLogoSVG { get; set; }

        [JsonProperty("solidLogoPNG")]
        public PlutoImage SolidLogoPNG { get; set; }

        [JsonProperty("featured")]
        public bool Featured { get; set; }

        [JsonProperty("featuredOrder")]
        public int FeaturedOrder { get; set; }

        [JsonProperty("favorite")]
        public bool Favorite { get; set; }

        [JsonProperty("timelines")]
        public List<PlutoTimeline> Timelines { get; set; }

        [JsonProperty("stitched")]
        public PlutoStitched Stitched { get; set; }
    }
}