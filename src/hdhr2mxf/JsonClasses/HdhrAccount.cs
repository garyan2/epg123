using Newtonsoft.Json;

namespace GaRyan2.SiliconDustApi
{
    public class HdhrAccount
    {
        public override string ToString()
        {
            return $"{AccountEmail} DVR Service: {(DvrActive > 0 ? "active" : "inactive")}";
        }

        [JsonProperty("AccountEmail")]
        public string AccountEmail { get; set; }

        [JsonProperty("AccountDeviceIDs")]
        public string[] AccountDeviceIDs { get; set; }

        [JsonProperty("DvrActive")]
        public int DvrActive { get; set; }

        [JsonProperty("AccountState")]
        public string AccountState { get; set; }
    }
}