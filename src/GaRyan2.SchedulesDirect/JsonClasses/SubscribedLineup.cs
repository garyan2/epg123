using Newtonsoft.Json;
using System.Collections.Generic;

namespace GaRyan2.SchedulesDirectAPI
{
    public class LineupResponse : BaseResponse
    {
        [JsonProperty("lineups")]
        [JsonConverter(typeof(SingleOrListConverter<SubscribedLineup>))]
        public List<SubscribedLineup> Lineups { get; set; }
    }

    public class SubscribedLineup
    {
        public override string ToString()
        {
            return $"{Name} ({Location})";
        }

        [JsonProperty("lineup")]
        public string Lineup { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("transport")]
        public string Transport { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }
    }
}