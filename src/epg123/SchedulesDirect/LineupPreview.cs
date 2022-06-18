using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static List<LineupPreviewChannel> GetLineupPreviewChannels(string lineup)
        {
            var ret = GetSdApiResponse<List<LineupPreviewChannel>>("GET", $"lineups/preview/{lineup}");
            if (ret != null) Logger.WriteVerbose($"Successfully retrieved the channels in lineup {lineup} for preview.");
            else Logger.WriteError($"Did not receive a response from Schedules Direct for retrieval of lineup {lineup} for preview.");
            return ret;
        }
    }

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
