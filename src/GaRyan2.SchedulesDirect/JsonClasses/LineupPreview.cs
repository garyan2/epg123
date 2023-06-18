using Newtonsoft.Json;

namespace GaRyan2.SchedulesDirectAPI
{
    public class LineupPreviewChannel
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("callsign")]
        public string Callsign { get; set; }

        [JsonProperty("affiliate")]
        public string Affiliate { get; set; }
    }
}