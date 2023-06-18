using Newtonsoft.Json;

namespace GaRyan2.SchedulesDirectAPI
{
    public class AddRemoveLineupResponse : BaseResponse
    {
        [JsonProperty("changesRemaining")]
        public int ChangesRemaining { get; set; }
    }
}