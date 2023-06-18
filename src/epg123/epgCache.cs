using GaRyan2.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace epg123
{
    public static class epgCache
    {
        public static Dictionary<string, epgJsonCache> JsonFiles;
        private static bool isDirty;

        public static void LoadCache()
        {
            if ((JsonFiles = Helper.ReadJsonFile(Helper.Epg123CacheJsonPath, typeof(Dictionary<string, epgJsonCache>))) == null)
            {
                JsonFiles = new Dictionary<string, epgJsonCache>();
                if (File.Exists(Helper.Epg123CacheJsonPath)) Logger.WriteInformation("The cache file appears to be corrupted and will need to be rebuilt.");
            }
        }

        public static void WriteCache()
        {
            if (!isDirty || JsonFiles.Count <= 0) return;
            CleanDictionary();
            if (!Helper.WriteJsonFile(JsonFiles, Helper.Epg123CacheJsonPath))
            {
                Logger.WriteInformation("Deleting cache file to be rebuilt on next update.");
                Helper.DeleteFile(Helper.Epg123CacheJsonPath);
            }
            CloseCache();
        }

        public static void CloseCache()
        {
            JsonFiles.Clear();
        }

        public static string GetAsset(string md5)
        {
            JsonFiles[md5].Current = true;
            return JsonFiles[md5].JsonEntry;
        }

        private static string CleanJsonText(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                json = Regex.Replace(json, "\"\\w+?\":null,?", string.Empty);
                json = Regex.Replace(json, "\"\\w+?\":\"\",?", string.Empty);
                json = Regex.Replace(json, "\"\\w+?\":false,?", string.Empty);
                json = Regex.Replace(json, ",}", "}");
                json = Regex.Replace(json, ",]", "]");
            }
            return json;
        }

        public static void AddAsset(string md5, string json)
        {
            if (JsonFiles.ContainsKey(md5)) return;

            // reduce the size of the string by removing nulls, empty strings, and false booleans
            json = CleanJsonText(json);

            // store
            var epgJson = new epgJsonCache()
            {
                JsonEntry = json,
                Current = true
            };
            JsonFiles.Add(md5, epgJson);
            isDirty = true;
        }

        public static void UpdateAssetImages(string md5, string json)
        {
            if (!JsonFiles.ContainsKey(md5))
            {
                AddAsset(md5, null);
            }

            // reduce the size of the string by removing nulls and empty strings
            json = CleanJsonText(json);

            // store
            JsonFiles[md5].Images = json;
            isDirty = true;
        }

        public static void UpdateAssetJsonEntry(string md5, string json)
        {
            if (!JsonFiles.ContainsKey(md5))
            {
                AddAsset(md5, json);
            }

            // reduce the size of the string by removing nulls and empty strings
            json = CleanJsonText(json);

            // store
            JsonFiles[md5].JsonEntry = json;
            isDirty = true;
        }

        public static void CleanDictionary()
        {
            var keysToDelete = (from asset in JsonFiles where !asset.Value.Current select asset.Key).ToList();
            foreach (var key in keysToDelete)
            {
                JsonFiles.Remove(key);
            }
            Logger.WriteInformation($"{keysToDelete.Count} entries deleted from the cache file during cleanup.");
        }
    }

    public class epgJsonCache
    {
        [JsonProperty("jsonEntry")]
        public string JsonEntry { get; set; }

        [JsonProperty("images")]
        public string Images { get; set; }

        [JsonIgnore]
        public bool Current { get; set; }
    }
}