using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace epg123.SchedulesDirect
{
    public static partial class SdApi
    {
        public static List<ProgramMetadata> GetArtwork(string[] request)
        {
            var dtStart = DateTime.Now;
            var sr = GetRequestResponse(methods.POST, "metadata/programs", request, false);
            if (sr == null)
            {
                Logger.WriteInformation($"Did not receive a response from Schedules Direct for artwork info of {request.Length,3} programs. ({GetStringTimeAndByteLength(DateTime.Now - dtStart)})");
                return null;
            }

            try
            {
                Logger.WriteVerbose($"Successfully retrieved artwork info for {request.Length,3} programs. ({GetStringTimeAndByteLength(DateTime.Now - dtStart, sr.Length)})");
                return JsonConvert.DeserializeObject<List<ProgramMetadata>>(sr, jSettings);
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"GetArtwork() Unknown exception thrown. Message: {ex.Message}");
            }
            return null;
        }
    }

    public class ProgramMetadata
    {
        [JsonProperty("programID")]
        public string ProgramId { get; set; }

        [JsonProperty("data")]
        [JsonConverter(typeof(SingleOrArrayConverter<ProgramArtwork>))]
        public List<ProgramArtwork> Data { get; set; }
    }

    public class ProgramArtwork
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("aspect")]
        public string Aspect { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("primary")]
        public string Primary { get; set; }

        [JsonProperty("tier")]
        public string Tier { get; set; }
    }

    class SingleOrArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<T>));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            return token.Type == JTokenType.Array ? token.ToObject<List<T>>() : new List<T> { token.ToObject<T>() };
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
