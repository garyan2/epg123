using Newtonsoft.Json;

namespace GaRyan2.SiliconDustApi
{
    public class HdhrChannel
    {
        public override string ToString()
        {
            return $"{GuideNumber} {GuideName}";
        }

        public int Number
        {
            get
            {
                var numbers = GuideNumber.Split('.');
                return int.Parse(numbers[0]);
            }
        }
        public int Subnumber
        {
            get
            {
                var numbers = GuideNumber.Split('.');
                return (numbers.Length <= 1 ? 0 : int.Parse(numbers[1]));
            }
        }

        [JsonProperty("GuideNumber")]
        public string GuideNumber { get; set; }

        [JsonProperty("GuideName")]
        public string GuideName { get; set; }

        [JsonProperty("VideoCodec")]
        public string VideoCodec { get; set; }

        [JsonProperty("AudioCodec")]
        public string AudioCodec { get; set; }

        [JsonProperty("HD")]
        public int Hd { get; set; }

        [JsonProperty("Favorite")]
        public int Favorite { get; set; }

        [JsonProperty("URL")]
        public string Url { get; set; }

        // the below items are returned with the ?tuning option in the request
        [JsonProperty("TransportStreamID")]
        public int TransportStreamId { get; set; }

        [JsonProperty("Modulation")]
        public string Modulation { get; set; }

        [JsonProperty("Frequency")]
        public int Frequency { get; set; }

        [JsonProperty("ProgramNumber")]
        public int ProgramNumber { get; set; }

        [JsonProperty("OriginalNetworkID")]
        public int OriginalNetworkId { get; set; }
    }
}