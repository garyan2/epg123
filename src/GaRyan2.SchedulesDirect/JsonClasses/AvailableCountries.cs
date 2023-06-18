using Newtonsoft.Json;

namespace GaRyan2.SchedulesDirectAPI
{
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