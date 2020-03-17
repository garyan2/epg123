using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace epg123
{
    static class epgCache
    {
        public static Dictionary<string, epgJsonCache> JsonFiles = new Dictionary<string, epgJsonCache>();
        private static bool isDirty = false;

        public static void LoadCache()
        {
            if (File.Exists(Helper.Epg123CacheJsonPath))
            {
                using (StreamReader reader = File.OpenText(Helper.Epg123CacheJsonPath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    JsonFiles = (Dictionary<string, epgJsonCache>)serializer.Deserialize(reader, typeof(Dictionary<string, epgJsonCache>));
                }
            }
        }

        public static void WriteCache()
        {
            if (isDirty && JsonFiles.Count > 0)
            {
                CleanDictionary();
                using (StreamWriter writer = File.CreateText(Helper.Epg123CacheJsonPath))
                {
                    try
                    {
                        JsonSerializer serializer = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };
                        serializer.Serialize(writer, JsonFiles);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteWarning("Failed to write cache file to the cache folder. Message: " + ex.Message);
                    }
                }
            }
        }

        public static string GetAsset(string md5)
        {
            JsonFiles[md5].LastUsedDate = DateTime.UtcNow;
            return JsonFiles[md5].JsonEntry;
        }

        public static void AddAsset(string md5, string json)
        {
            // reduce the size of the string by removing nulls, empty strings, and false booleans
            json = Regex.Replace(json, "\"\\w+?\":null,?", string.Empty);
            json = Regex.Replace(json, "\"\\w+?\":\"\",?", string.Empty);
            json = Regex.Replace(json, "\"\\w+?\":false,?", string.Empty);

            // store
            epgJsonCache epgJson = new epgJsonCache()
            {
                JsonEntry = json,
                LastUsedDate = DateTime.UtcNow
            };
            JsonFiles.Add(md5, epgJson);
            isDirty = true;
        }

        public static void CleanDictionary()
        {
            List<string> keysToDelete = new List<string>();
            foreach (KeyValuePair<string, epgJsonCache> asset in JsonFiles)
            {
                if (asset.Value.LastUsedDate < sdJson2mxf.startTime)
                {
                    keysToDelete.Add(asset.Key);
                }
            }
            foreach (string key in keysToDelete)
            {
                JsonFiles.Remove(key);
            }
            Logger.WriteInformation(string.Format("{0} entries deleted from the cache file during cleanup.", keysToDelete.Count));
        }
    }

    public class epgJsonCache
    {
        [JsonProperty("jsonEntry")]
        public string JsonEntry { get; set; }

        [JsonProperty("lastUsedDate")]
        public DateTime LastUsedDate { get; set; }
    }
}
