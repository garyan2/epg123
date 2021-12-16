using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace tokenServer
{
    public static class JsonImageCache
    {
        public static Dictionary<string, CacheImage> ImageCache;
        private static readonly object _cacheLock = new object();
        public static bool cacheImages;
        public static int cacheRetention;

        static JsonImageCache()
        {
            Load();
        }

        private static void Load()
        {
            if (File.Exists(Helper.Epg123ImageCachePath))
            {
                using (var reader = File.OpenText(Helper.Epg123ImageCachePath))
                {
                    var serializer = new JsonSerializer();
                    ImageCache = (Dictionary<string, CacheImage>)serializer.Deserialize(reader, typeof(Dictionary<string, CacheImage>));
                }
            }
            else ImageCache = new Dictionary<string, CacheImage>();
        }

        public static void Cleanup()
        {
            var markedForDelete = ImageCache.Where(arg => arg.Value.LastUsed + TimeSpan.FromDays(cacheRetention) < DateTime.Now)
                .Select(arg => arg.Key).ToList();
            foreach (var image in markedForDelete)
            {
                var path = $"{Helper.Epg123ImageCache}\\{image.Substring(0, 1)}\\{image}";
                try
                {
                    if (File.Exists(path)) File.Delete(path);
                    ImageCache.Remove(image);
                }
                catch { }
            }
        }

        public static void Save()
        {
            lock (_cacheLock)
            {
                Cleanup();
                using (var writer = File.CreateText(Helper.Epg123ImageCachePath))
                {
                    var serializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.None };
                    serializer.Serialize(writer, ImageCache);
                }
            }
        }

        public static FileInfo GetCachedImage(string filename)
        {
            // remove "/image/" from beginning
            filename = filename.Substring(7);
            var location = $"{Helper.Epg123ImageCache}\\{filename.Substring(0, 1)}\\{filename}";
            lock (_cacheLock)
            {
                if (ImageCache.ContainsKey(filename))
                {
                    if (File.Exists(location))
                    {
                        ImageCache[filename].LastUsed = DateTime.Now;
                        return new FileInfo(location);
                    }
                    ImageCache.Remove(filename);
                }

                if (File.Exists(location))
                {
                    ImageCache.Add(filename, new CacheImage { LastUsed = DateTime.Now });
                    return new FileInfo(location);
                }
                return null;
            }
        }

        public static void AddImageToCache(string filename, DateTime lastModified)
        {
            lock (_cacheLock)
            {
                if (ImageCache.ContainsKey(filename))
                {
                    ImageCache[filename].LastUsed = DateTime.Now;
                    ImageCache[filename].LastModified = lastModified;
                }
                else ImageCache.Add(filename, new CacheImage { LastUsed = DateTime.Now, LastModified = lastModified});
            }
        }

        public static bool IsImageRecent(string filename, DateTime ifModifiedSince)
        {
            lock (_cacheLock)
            {
                if (!ImageCache.ContainsKey(filename)) return false;
                if (ImageCache[filename].LastModified == DateTime.MinValue)
                {
                    var info = new FileInfo($"{Helper.Epg123ImageCache}\\{filename.Substring(0, 1)}\\{filename}");
                    ImageCache[filename].LastModified = info.LastWriteTimeUtc;
                }
                if (ifModifiedSince == DateTime.MinValue && DateTime.UtcNow - ImageCache[filename].LastModified < TimeSpan.FromDays(30))
                {
                    return true;
                }
                if (ImageCache[filename].LastUsed.ToLocalTime() + TimeSpan.FromHours(24) > DateTime.Now) return true;
            }
            return false;
        }
    }

    public class CacheImage
    {
        [JsonProperty("LastUsed")]
        public DateTime LastUsed { get; set; }

        [JsonProperty("LastModified")]
        public DateTime LastModified { get; set; }
    }
}
