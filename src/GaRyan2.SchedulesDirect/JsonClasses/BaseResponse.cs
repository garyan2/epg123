using Newtonsoft.Json;
using System;

namespace GaRyan2.SchedulesDirectAPI
{
    public class BaseResponse
    {
        [JsonProperty("response")]
        public string Response { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("serverID")]
        public string ServerId { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("datetime")]
        public DateTime Datetime { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }
    }
}