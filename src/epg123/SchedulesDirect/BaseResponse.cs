using System;
using Newtonsoft.Json;

namespace epg123.SchedulesDirect
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
        public DateTime Datetime { get; set; } = DateTime.MinValue;
        public bool ShouldSerializeDatetime() { return Datetime != DateTime.MinValue; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }
    }
}
