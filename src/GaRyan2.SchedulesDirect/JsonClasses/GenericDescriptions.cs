using Newtonsoft.Json;

namespace GaRyan2.SchedulesDirectAPI
{
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