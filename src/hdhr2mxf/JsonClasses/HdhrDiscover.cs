using Newtonsoft.Json;

namespace GaRyan2.SiliconDustApi
{
    public class HdhrDiscover
    {
        public override string ToString()
        {
            return $"{DeviceId ?? StorageID} ({LocalIp})";
        }

        [JsonProperty("DeviceID")]
        public string DeviceId { get; set; }

        [JsonProperty("StorageID")]
        public string StorageID { get; set; }

        [JsonProperty("LocalIP")]
        public string LocalIp { get; set; }

        [JsonProperty("BaseURL")]
        public string BaseUrl { get; set; }

        [JsonProperty("DiscoverURL")]
        public string DiscoverUrl { get; set; }

        [JsonProperty("LineupURL")]
        public string LineupUrl { get; set; }

        [JsonProperty("StorageURL")]
        public string StorageURL { get; set; }
    }
}