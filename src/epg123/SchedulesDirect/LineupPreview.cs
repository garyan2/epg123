using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static List<LineupPreviewChannel> GetLineupPreviewChannels(string lineup)
        {
            var sr = GetRequestResponse(methods.GET, $"lineups/preview/{lineup}");
            if (sr == null)
            {
                Logger.WriteError($"Did not receive a response from Schedules Direct for retrieval of lineup {lineup} for preview.");
                return null;
            }

            try
            {
                Logger.WriteVerbose($"Successfully retrieved the channels in lineup {lineup} for preview.");
                return JsonConvert.DeserializeObject<List<LineupPreviewChannel>>(sr, jSettings);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"GetLineupPreviewChannels() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
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
