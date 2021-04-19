using System;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static ClientVersion GetClientVersion()
        {
            var sr = GetRequestResponse(methods.GET, $"version/{userAgent}", null, false);
            if (sr == null)
            {
                Logger.WriteError("Did not receive a response from Schedules Direct for a version check.");
                return null;
            }

            try
            {
                var ret = JsonConvert.DeserializeObject<ClientVersion>(sr);
                switch (ret.Code)
                {
                    case 0:
                        if (ret.Version == grabberVersion) return ret;
                        if (Logger.EventId == 0) Logger.EventId = 1;
                        Logger.WriteInformation($"epg123 is not up to date. Latest version is {ret.Version} and can be downloaded from http://epg123.garyan2.net/download.");
                        return ret;
                    default:
                        Logger.WriteInformation($"Failed version check. code: {ret.Code} , message: {ret.Message} , datetime: {ret.Datetime:s}Z");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"GetClientVersion() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }
    }

    public class ClientVersion : BaseResponse
    {
        [JsonProperty("client")]
        public string Client { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
