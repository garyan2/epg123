using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static UserStatus GetUserStatus()
        {
            var sr = GetRequestResponse(methods.GET, "status");
            if (sr == null)
            {
                Logger.WriteError("Did not receive a response from Schedules Direct for a status request.");
                return null;
            }

            try
            {
                var ret = JsonConvert.DeserializeObject<UserStatus>(sr);
                if (ret.Code == 0)
                {
                    Logger.WriteVerbose($"Status request successful. account expires: {ret.Account.Expires:s}Z , lineups: {ret.Lineups.Count}/{ret.Account.MaxLineups} , lastDataUpdate: {ret.LastDataUpdate:s}Z");
                    Logger.WriteVerbose($"system status: {ret.SystemStatus[0].Status} , message: {ret.SystemStatus[0].Message}");
                    MaxLineups = ret.Account.MaxLineups;

                    var expires = ret.Account.Expires - DateTime.UtcNow;
                    if (expires < TimeSpan.FromDays(7.0))
                    {
                        Logger.WriteWarning($"Your Schedules Direct account expires in {expires.Days:D2} days {expires.Hours:D2} hours {expires.Minutes:D2} minutes.");
                    }
                    return ret;
                }
                Logger.WriteError($"Failed to get account status. code: {ret.Code} , message: {ret.Message}");
            }
            catch (Exception ex)
            {
                Logger.WriteError($"SdUserStatusResponse() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }
    }

    public class UserStatus : BaseResponse
    {
        [JsonProperty("account")]
        public StatusAccount Account { get; set; }

        [JsonProperty("lineups")]
        public IList<StatusLineup> Lineups { get; set; }

        [JsonProperty("lastDataUpdate")]
        public DateTime LastDataUpdate { get; set; }

        [JsonProperty("notifications")]
        public string[] Notifications { get; set; }

        [JsonProperty("systemStatus")]
        public List<SystemStatus> SystemStatus { get; set; }
    }

    public class StatusAccount
    {
        [JsonProperty("expires")]
        public DateTime Expires { get; set; }

        [JsonProperty("messages")]
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
