using System.Collections.Generic;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static Dictionary<string, string> GetTransmitters(string country)
        {
            var ret = GetSdApiResponse<Dictionary<string, string>>("GET", $"transmitters/{country}");
            if (ret != null) Logger.WriteVerbose($"Successfully retrieved list of available transmitters from Schedules Direct for country {country}.");
            else Logger.WriteError($"Did not receive a response from Schedules Direct for a list of available transmitters for country {country}.");
            return ret;
        }
    }
}
