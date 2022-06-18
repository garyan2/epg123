using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static UserStatus GetUserStatus()
        {
            var ret = GetSdApiResponse<UserStatus>("GET", "status");
            if (ret != null)
            {
                Logger.WriteVerbose($"Status request successful. account expires: {ret.Account.Expires:s}Z , lineups: {ret.Lineups.Count}/{ret.Account.MaxLineups} , lastDataUpdate: {ret.LastDataUpdate:s}Z");
                Logger.WriteVerbose($"System status: {ret.SystemStatus[0].Status} , message: {ret.SystemStatus[0].Message}");
                MaxLineups = ret.Account.MaxLineups;

                var expires = ret.Account.Expires - DateTime.UtcNow;
                if (expires >= TimeSpan.FromDays(7.0)) return ret;
                Logger.WriteWarning($"Your Schedules Direct account expires in {expires.Days:D2} days {expires.Hours:D2} hours {expires.Minutes:D2} minutes.");
                Logger.WriteInformation("*** Renew your Schedules Direct membership at https://schedulesdirect.org. ***");
            }
            else Logger.WriteError("Did not receive a response from Schedules Direct for a status request.");
            return ret;
        }
    }

    public class UserStatus : BaseResponse
    {
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
