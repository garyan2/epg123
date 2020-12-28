using System;
using Newtonsoft.Json;

namespace epg123.SchedulesDirectAPI
{
    public class sdError
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
    }
}
