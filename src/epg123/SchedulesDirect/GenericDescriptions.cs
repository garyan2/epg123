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
            var sr = GetRequestResponse(methods.POST, "metadata/description", request);
            if (sr == null)
            {
                Logger.WriteInformation($"Did not receive a response from Schedules Direct for {request.Length,3} generic program descriptions. ({GetStringTimeAndByteLength(DateTime.Now - dtStart)})");
                return null;
            }

            try
            {
                Logger.WriteVerbose($"Successfully retrieved {request.Length,3} generic program descriptions. ({GetStringTimeAndByteLength(DateTime.Now - dtStart, sr.Length)})");
                return JsonConvert.DeserializeObject<Dictionary<string, GenericDescription>>(sr);
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"GetGenericDescriptions() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
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
