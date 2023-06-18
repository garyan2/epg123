using Newtonsoft.Json;
using System.Collections.Generic;

namespace GaRyan2.SchedulesDirectAPI
{
    public class Headend
    {
        [JsonProperty("headend")]
        public string HeadendId { get; set; }

        [JsonProperty("transport")]
        public string Transport { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("lineups")]
        [JsonConverter(typeof(SingleOrListConverter<HeadendLineup>))]
        public List<HeadendLineup> Lineups { get; set; }
    }

    public class HeadendLineup
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("lineup")]
        public string Lineup { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
}