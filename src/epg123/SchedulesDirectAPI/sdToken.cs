using System;
using Newtonsoft.Json;

namespace epg123.SchedulesDirectAPI
{
    public class SdTokenRequest
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string PasswordHash { get; set; }
    }

    public class SdTokenResponse
    {
        //[JsonProperty("response")]
        //public string Response { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("serverID")]
        public string ServerId { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("datetime")]
        public DateTime DateTime { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
