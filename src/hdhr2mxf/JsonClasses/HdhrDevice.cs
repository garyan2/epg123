using Newtonsoft.Json;

namespace GaRyan2.SiliconDustApi
{
    public class HdhrDevice
    {
        public override string ToString()
        {
            return $"{FriendlyName} {ModelNumber} ({DeviceId})";
        }

        public string MxfLineupID => $"EPG123-HDHR2MXF-{ModelNumber.Substring(ModelNumber.Length - 2).Replace("4K", "US")}";
        public string MxfLineupName => $"EPG123 HDHR-{ModelNumber.Substring(ModelNumber.Length - 2).Replace("4K", "US")} to MXF Converter";

        [JsonProperty("FriendlyName")]
        public string FriendlyName { get; set; }

        [JsonProperty("ModelNumber")]
        public string ModelNumber { get; set; }

        [JsonProperty("Legacy")]
        public int Legacy { get; set; }

        [JsonProperty("FirmwareName")]
        public string FirmwareName { get; set; }

        [JsonProperty("FirmwareVersion")]
        public string FirmwareVersion { get; set; }

        [JsonProperty("DeviceID")]
        public string DeviceId { get; set; }

        [JsonProperty("DeviceAuth")]
        public string DeviceAuth { get; set; }

        [JsonProperty("BaseURL")]
        public string BaseUrl { get; set; }

        [JsonProperty("LineupURL")]
        public string LineupUrl { get; set; }

        [JsonProperty("TunerCount")]
        public int TunerCount { get; set; }

        [JsonProperty("StorageID")]
        public string StorageID { get; set; }

        [JsonProperty("StorageURL")]
        public string StorageURL { get; set; }

        [JsonProperty("TotalSpace")]
        public long TotalSpace { get; set; }

        [JsonProperty("FreeSpace")]
        public long FreeSpace { get; set; }

        [JsonProperty("Version")]
        public string Version { get; set; }
    }
}