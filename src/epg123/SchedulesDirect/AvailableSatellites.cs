using System;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static dynamic GetAvailableSatellites()
        {
            var sr = GetRequestResponse(methods.GET, "available/dvb-s", null, false);
            if (sr == null)
            {
                Logger.WriteError("Did not receive a response from Schedules Direct for a list of available satellites.");
                return null;
            }

            try
            {
                Logger.WriteVerbose("Successfully retrieved list of available satellites from Schedules Direct.");
                return JsonConvert.DeserializeObject<dynamic>(sr, jSettings);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"GetAvailableSatellites() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }
    }
}
