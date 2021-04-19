using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static Dictionary<string, string> GetTransmitters(string country)
        {
            var sr = GetRequestResponse(methods.GET, $"transmitters/{country}", null, false);
            if (sr == null)
            {
                Logger.WriteError($"Did not receive a response from Schedules Direct for a list of available transmitters for country {country}.");
                return null;
            }

            try
            {
                Logger.WriteVerbose($"Successfully retrieved list of available transmitters from Schedules Direct for country {country}.");
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(sr);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"GetTransmitters() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }
    }
}
