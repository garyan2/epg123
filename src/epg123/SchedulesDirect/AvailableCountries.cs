using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public partial class SdApi
    {
        public static Dictionary<string, List<Country>> GetAvailableCountries()
        {
            var sr = GetRequestResponse(methods.GET, "available/countries", null, false);
            if (sr == null)
            {
                Logger.WriteError("Did not receive a response from Schedules Direct for a list of available countries.");
                return null;
            }

            try
            {
                Logger.WriteVerbose("Successfully retrieved list of available countries from Schedules Direct.");
                return JsonConvert.DeserializeObject<Dictionary<string, List<Country>>>(sr, jSettings);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"GetAvailableCountries() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }
    }

    public class Country
    {
        [JsonProperty("fullName")]
        public string FullName { get; set; }

        [JsonProperty("shortName")]
        public string ShortName { get; set; }

        [JsonProperty("postalCodeExample")]
        public string PostalCodeExample { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("onePostalCode")]
        public bool OnePostalCode { get; set; }
    }
}
