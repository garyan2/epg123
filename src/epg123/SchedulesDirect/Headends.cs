using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static List<Headend> GetHeadends(string country, string postalcode)
        {
            var sr = GetRequestResponse(methods.GET, $"headends?country={country}&postalcode={postalcode}");
            if (sr == null)
            {
                Logger.WriteError($"Failed to get a response from Schedules Direct for the headends of {country} and postal code {postalcode}.");
                return null;
            }

            try
            {
                Logger.WriteVerbose($"Successfully retrieved the headends for {country} and postal code {postalcode}.");
                return JsonConvert.DeserializeObject<List<Headend>>(sr, jSettings);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"GetHeadends() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }
    }

    public class Headend
    {
        [JsonProperty("headend")]
        public string HeadendId { get; set; }

        [JsonProperty("transport")]
        public string Transport { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("lineups")]
        public List<HeadendLineup> Lineups { get; set; }
    }

    public class HeadendLineup
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("lineup")]
        public string Lineup { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
}
