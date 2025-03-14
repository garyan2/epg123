using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GaRyan2.SchedulesDirectAPI
{
    public class UserStatus : BaseResponse
    {
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [JsonProperty("account")]
        public StatusAccount Account { get; set; }

        [JsonProperty("lineups")]
        [JsonConverter(typeof(SingleOrListConverter<StatusLineup>))]
        public List<StatusLineup> Lineups { get; set; }

        [JsonProperty("lastDataUpdate")]
        public DateTime LastDataUpdate { get; set; }

        [JsonProperty("notifications")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] Notifications { get; set; }

        [JsonProperty("systemStatus")]
        [JsonConverter(typeof(SingleOrListConverter<SystemStatus>))]
        public List<SystemStatus> SystemStatus { get; set; }

        public DateTime TokenExpires => epoch.AddSeconds(tokenExpiresEpoch);

        [JsonProperty("tokenExpires")]
        public long tokenExpiresEpoch;
    }

    public class StatusAccount
    {
        [JsonProperty("expires")]
        public DateTime Expires { get; set; }

        [JsonProperty("messages")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public string[] Messages { get; set; }

        [JsonProperty("maxLineups")]
        public int MaxLineups { get; set; }
    }

    public class StatusLineup
    {
        [JsonProperty("lineup")]
        public string Lineup { get; set; }

        [JsonProperty("modified")]
        public string Modified { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }
    }

    public class SystemStatus
    {
        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}