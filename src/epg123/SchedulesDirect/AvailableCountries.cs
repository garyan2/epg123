using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public partial class SdApi
    {
        public static Dictionary<string, List<Country>> GetAvailableCountries()
        {
            var ret = GetSdApiResponse<Dictionary<string, List<Country>>>("GET", "available/countries");
            if (ret != null) Logger.WriteVerbose("Successfully retrieved list of available countries from Schedules Direct.");
            else Logger.WriteError("Did not receive a response from Schedules Direct for a list of available countries.");
            return ret;
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
