using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace epg123
{
    public static class epgCache
    {
        public static Dictionary<string, epgJsonCache> JsonFiles = new Dictionary<string, epgJsonCache>();
        private static bool isDirty;
        private const string CacheFileUri = "/epg123cache.json";

        public static void LoadCache()
        {
            try
            {
                if (File.Exists(Helper.Epg123CompressCachePath))
                {
                    var zip = new CompressXmlFiles();
                    using (var reader = new StreamReader(zip.GetBackupFileStream(CacheFileUri, Helper.Epg123CompressCachePath)))
                    {
                        var serializer = new JsonSerializer();
                        JsonFiles = (Dictionary<string, epgJsonCache>)serializer.Deserialize(reader, typeof(Dictionary<string, epgJsonCache>));
                    }
                    zip.ClosePackage();
                }
                else if (File.Exists(Helper.Epg123CacheJsonPath))
                {
                    using (var reader = File.OpenText(Helper.Epg123CacheJsonPath))
                    {
                        var serializer = new JsonSerializer();
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
            if (!isDirty || JsonFiles.Count <= 0) return;
            CleanDictionary();
            try
            {
                //if (new ComputerInfo().AvailablePhysicalMemory < (ulong)Math.Pow(1024, 3)) // disable compression of cache file
                {
                    using (var writer = File.CreateText(Helper.Epg123CacheJsonPath))
                    {
                        var serializer = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };
                        serializer.Serialize(writer, JsonFiles);
                    }

                    Helper.DeleteFile(Helper.Epg123CompressCachePath);
                }
                //else
                //{
                //    var stream = new MemoryStream();
                //    using (var swriter = new StreamWriter(stream))
                //    {
                //        using (var jwriter = new JsonTextWriter(swriter))
                //        {
                //            var serializer = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };
                //            serializer.Serialize(jwriter, JsonFiles);
                //            swriter.Flush();
                //            stream.Seek(0, SeekOrigin.Begin);

                //            CompressXmlFiles.CompressSingleStreamToFile(stream, cacheFileUri, Helper.Epg123CompressCachePath);
                //        }
                //    }
                //    if (File.Exists(Helper.Epg123CacheJsonPath))
                //    {
                //        File.Delete(Helper.Epg123CacheJsonPath);
                //    }
                //}
            }
            catch (Exception ex)
            {
                Logger.WriteInformation("Failed to write cache file to the cache folder. Message: " + ex.Message);
                Logger.WriteInformation("Deleting cache file to be rebuilt on next update.");
                Helper.DeleteFile(Helper.Epg123CompressCachePath);
                Helper.DeleteFile(Helper.Epg123CacheJsonPath);
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
            if (json != null)
            {
                json = Regex.Replace(json, "\"\\w+?\":null,?", string.Empty);
                json = Regex.Replace(json, "\"\\w+?\":\"\",?", string.Empty);
                json = Regex.Replace(json, "\"\\w+?\":false,?", string.Empty);
            }

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
                Logger.WriteInformation($"Failed to update asset image for program with MD5 {md5}.");
                return;
            }

            // reduce the size of the string by removing nulls and empty strings
            json = Regex.Replace(json, "\"\\w+?\":null,?", string.Empty);
            json = Regex.Replace(json, "\"\\w+?\":\"\",?", string.Empty);

            // store
            JsonFiles[md5].Images = json;
            isDirty = true;
        }

        public static void UpdateAssetJsonEntry(string md5, string json)
        {
            if (!JsonFiles.ContainsKey(md5))
            {
                Logger.WriteInformation($"Failed to update asset json for program with MD5 {md5}.");
                return;
            }

            // reduce the size of the string by removing nulls and empty strings
            json = Regex.Replace(json, "\"\\w+?\":null,?", string.Empty);
            json = Regex.Replace(json, "\"\\w+?\":\"\",?", string.Empty);

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
