using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static LineupResponse GetSubscribedLineups()
        {
            var ret = GetSdApiResponse<LineupResponse>("GET", "lineups");
            if (ret != null) Logger.WriteVerbose("Successfully requested listing of subscribed lineups from Schedules Direct.");
            else Logger.WriteError("Did not receive a response from Schedules Direct for list of subscribed lineups.");
            return ret;
        }
    }

    public class LineupResponse : BaseResponse
    {
        [JsonProperty("lineups")]
        [JsonConverter(typeof(SingleOrListConverter<SubscribedLineup>))]
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
