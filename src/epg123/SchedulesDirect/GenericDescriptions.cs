using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static Dictionary<string, GenericDescription> GetGenericDescriptions(string[] request)
        {
            var dtStart = DateTime.Now;
            var ret = GetSdApiResponse<Dictionary<string, GenericDescription>>("POST", "metadata/description", request);
            if (ret != null) Logger.WriteVerbose($"Successfully retrieved {request.Length,3} generic program descriptions. ({(DateTime.Now - dtStart):G})");
            else Logger.WriteInformation($"Did not receive a response from Schedules Direct for {request.Length,3} generic program descriptions. ({(DateTime.Now - dtStart):G})");
            return ret;
        }
    }

    public class GenericDescription : BaseResponse
    {
        [JsonProperty("startAirdate")]
        public string StartAirdate { get; set; }

        [JsonProperty("description100")]
        public string Description100 { get; set; }

        [JsonProperty("description1000")]
        public string Description1000 { get; set; }
    }
}
