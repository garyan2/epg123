using Newtonsoft.Json;
using System;

namespace GaRyan2.SchedulesDirectAPI
{
    public class TokenRequest
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string PasswordHash { get; set; }

        [JsonProperty("newToken")]
        public bool NewToken { get; set; }
        public bool ShouldSerializeNewToken() => NewToken;
    }

    public class TokenResponse : BaseResponse
    {
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public DateTime TokenExpires => epoch.AddSeconds(tokenExpiresEpoch);

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("tokenExpires")]
        public long tokenExpiresEpoch { get; set; }
    }
}