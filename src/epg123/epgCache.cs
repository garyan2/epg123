using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.Devices;
using Newtonsoft.Json;

namespace epg123
{
    static class epgCache
    {
        public static Dictionary<string, epgJsonCache> JsonFiles = new Dictionary<string, epgJsonCache>();
        private static bool isDirty = false;
        private static string cacheFileUri = "/epg123cache.json";

        public static void LoadCache()
        {
            try
            {
                if (File.Exists(Helper.Epg123CompressCachePath))
                {
                    using (StreamReader reader = new StreamReader(CompressXmlFiles.GetBackupFileStream(cacheFileUri, Helper.Epg123CompressCachePath)))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        JsonFiles = (Dictionary<string, epgJsonCache>)serializer.Deserialize(reader, typeof(Dictionary<string, epgJsonCache>));
                    }
                    CompressXmlFiles.ClosePackage();
                }
                else if (File.Exists(Helper.Epg123CacheJsonPath))
                {
                    using (StreamReader reader = File.OpenText(Helper.Epg123CacheJsonPath))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        JsonFiles = (Dictionary<string, epgJsonCache>)serializer.Deserialize(reader, typeof(Dictionary<string, epgJsonCache>));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteInformation("The cache file appears to be corrupted and will need to be rebuilt.");
                Logger.WriteInformation(ex.Message);
            }
        }

        public static void WriteCache()
        {
            if (isDirty && JsonFiles.Count > 0)
            {
                CleanDictionary();
                try
                {
                    if (true || new ComputerInfo().AvailablePhysicalMemory < (ulong)Math.Pow(1024, 3)) // disable compression of cache file
                    {
                        using (StreamWriter writer = File.CreateText(Helper.Epg123CacheJsonPath))
                        {
                            JsonSerializer serializer = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };
                            serializer.Serialize(writer, JsonFiles);
                        }
                        if (File.Exists(Helper.Epg123CompressCachePath))
                        {
                            File.Delete(Helper.Epg123CompressCachePath);
                        }
                    }
                    else
                    {
                        MemoryStream stream = new MemoryStream();
                        using (StreamWriter swriter = new StreamWriter(stream))
                        {
                            using (JsonTextWriter jwriter = new JsonTextWriter(swriter))
                            {
                                JsonSerializer serializer = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };
                                serializer.Serialize(jwriter, JsonFiles);
                                swriter.Flush();
                                stream.Seek(0, SeekOrigin.Begin);

                                CompressXmlFiles.CompressSingleStreamToFile(stream, cacheFileUri, Helper.Epg123CompressCachePath);
                            }
                        }
                        if (File.Exists(Helper.Epg123CacheJsonPath))
                        {
                            File.Delete(Helper.Epg123CacheJsonPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteInformation("Failed to write cache file to the cache folder. Message: " + ex.Message);
                    Logger.WriteInformation("Deleting cache file to be rebuilt on next update.");
                    try
                    {
                        if (File.Exists(Helper.Epg123CompressCachePath))
                        {
                            File.Delete(Helper.Epg123CompressCachePath);
                        }
                        if (File.Exists(Helper.Epg123CacheJsonPath))
                        {
                            File.Delete(Helper.Epg123CacheJsonPath);
                        }
                    }
                    catch { }
                }
            }
        }

        public static string GetAsset(string md5)
        {
            JsonFiles[md5].Current = true;
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
                Current = true
            };
            JsonFiles.Add(md5, epgJson);
            isDirty = true;
        }

        public static void UpdateAssetImages(string md5, string json)
        {
            // reduce the size of the string by removing nulls and empty strings
            json = Regex.Replace(json, "\"\\w+?\":null,?", string.Empty);
            json = Regex.Replace(json, "\"\\w+?\":\"\",?", string.Empty);

            // store
            JsonFiles[md5].Images = json;
            isDirty = true;
        }

        public static void UpdateAssetJsonEntry(string md5, string json)
        {
            // reduce the size of the string by removing nulls and empty strings
            json = Regex.Replace(json, "\"\\w+?\":null,?", string.Empty);
            json = Regex.Replace(json, "\"\\w+?\":\"\",?", string.Empty);

            // store
            JsonFiles[md5].JsonEntry = json;
            isDirty = true;
        }

        public static void CleanDictionary()
        {
            List<string> keysToDelete = new List<string>();
            foreach (KeyValuePair<string, epgJsonCache> asset in JsonFiles)
            {
                if (!asset.Value.Current)
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

        [JsonProperty("images")]
        public string Images { get; set; }

        [JsonIgnore]
        public bool Current { get; set; }
    }
}
