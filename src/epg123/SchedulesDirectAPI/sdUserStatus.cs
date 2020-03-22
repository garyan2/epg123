using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123
{
    public class sdUserStatusResponse
    {
        [JsonProperty("account")]
        public sdUserStatusAccount Account { get; set; }

        [JsonProperty("lineups")]
        public IList<sdUserStatusLineup> Lineups { get; set; }

        [JsonProperty("lastDataUpdate")]
        public string LastDataUpdate { get; set; }

        //[JsonProperty("notifications")]
        //public string[] Notifications { get; set; }

        [JsonProperty("systemStatus")]
        public IList<sdUserStatusSystemStatus> SystemStatus { get; set; }

        //[JsonProperty("serverID")]
        //public string ServerID { get; set; }

        //[JsonProperty("datetime")]
        //public string DateTime { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }
    }

    public class sdUserStatusAccount
    {
        [JsonProperty("expires")]
        public string Expires { get; set; }

        //[JsonProperty("messages")]
        //public string[] Messages { get; set; }

        [JsonProperty("maxLineups")]
        public int MaxLineups { get; set; }
    }

    public class sdUserStatusLineup
    {
        //[JsonProperty("lineup")]
        //public string Lineup { get; set; }

        //[JsonProperty("modified")]
        //public string Modified { get; set; }

        //[JsonProperty("uri")]
        //public string Uri { get; set; }

        //[JsonProperty("isDeleted")]
        //public bool IsDeleted { get; set; }
    }

    public class sdUserStatusSystemStatus
    {
        //[JsonProperty("date")]
        //public string Date { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
