using Newtonsoft.Json;

namespace GaRyan2.SiliconDustApi
{
    public class HdhrChannelDetail
    {
        public override string ToString()
        {
            return $"{GuideNumber} {GuideName}";
        }

        [JsonProperty("GuideNumber")]
        public string GuideNumber { get; set; }

        [JsonProperty("GuideName")]
        public string GuideName { get; set; }

        [JsonProperty("Affiliate")]
        public string Affiliate { get; set; }

        [JsonProperty("ImageURL")]
        public string ImageUrl { get; set; }
    }
}