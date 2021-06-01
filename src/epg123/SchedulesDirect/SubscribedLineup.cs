using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static LineupResponse GetSubscribedLineups()
        {
            var sr = GetRequestResponse(methods.GET, "lineups");
            if (sr == null)
            {
                Logger.WriteError("Did not receive a response from Schedules Direct for list of subscribed lineups.");
                return null;
            }

            try
            {
                var ret = JsonConvert.DeserializeObject<LineupResponse>(sr, jSettings);
                if (ret.Code == 0)
                {
                    Logger.WriteVerbose("Successfully requested listing of subscribed lineups from Schedules Direct.");
                    return ret;
                }
                Logger.WriteError($"Failed request for listing of subscribed lineups. code: {ret.Code} , message: {ret.Message}");
            }
            catch (Exception ex)
            {
                Logger.WriteError($"GetSubscribedLineups() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }
    }

    public class LineupResponse : BaseResponse
    {
        [JsonProperty("lineups")]
        public List<SubscribedLineup> Lineups { get; set; }
    }

    public class SubscribedLineup
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
