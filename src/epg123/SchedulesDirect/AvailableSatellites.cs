namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static dynamic GetAvailableSatellites()
        {
            var ret = GetSdApiResponse<dynamic>("GET", "available/dvb-s");
            if (ret != null) Logger.WriteVerbose("Successfully retrieved list of available satellites from Schedules Direct.");
            else Logger.WriteError("Did not receive a response from Schedules Direct for a list of available satellites.");
            return ret;
        }
    }
}
