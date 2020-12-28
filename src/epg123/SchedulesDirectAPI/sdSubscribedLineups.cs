using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123.SchedulesDirectAPI
{
    public class SdLineupResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        //[JsonProperty("serverID")]
        //public string ServerID { get; set; }

        //[JsonProperty("datetime")]
        //public string Datetime { get; set; }

        [JsonProperty("lineups")]
        public IList<SdLineup> Lineups { get; set; }
    }

    public class SdLineup
    {
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
