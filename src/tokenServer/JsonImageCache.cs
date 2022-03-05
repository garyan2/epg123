using System;
using System.Collections.Generic;
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
                var fi = new FileInfo(location);
                if (ImageCache.ContainsKey(filename))
                {
                    if (fi.Exists)
                    {
                        ImageCache[filename].LastUsed = DateTime.Now;
                        if (ImageCache[filename].ByteSize == 0) ImageCache[filename].ByteSize = fi.Length;
                        return fi;
                    }
                    ImageCache.Remove(filename);
                }

                if (!fi.Exists) return null;
                ImageCache.Add(filename, new CacheImage { LastUsed = DateTime.Now, ByteSize = fi.Length});
                return fi;
            }
        }

        public static void AddImageToCache(string filename, DateTime lastModified, long size)
        {
            lock (_cacheLock)
            {
                if (ImageCache.ContainsKey(filename))
                {
                    ImageCache[filename].LastUsed = DateTime.Now;
                    ImageCache[filename].LastModified = lastModified;
                    ImageCache[filename].ByteSize = size;
                }
                else ImageCache.Add(filename, new CacheImage { LastUsed = DateTime.Now, LastModified = lastModified, ByteSize = size});
            }
        }

        public static void GetAllImageSizes()
        {
            foreach (var image in ImageCache.Where(arg => arg.Value.ByteSize == 0))
            {
                var fi = new FileInfo($"{Helper.Epg123ImageCache}\\{image.Key.Substring(0, 1)}\\{image.Key}");
                if (fi.Exists)
                {
                    image.Value.ByteSize = fi.Length;
                }
            }
        }

        public static void AddImagesMissingInCacheFile()
        {
            var files = Directory.GetFiles($"{Helper.Epg123ImageCache}", "*.*", SearchOption.AllDirectories);
            foreach (var file in files.Where(arg => arg.EndsWith("jpg") || arg.EndsWith("png")))
            {
                var fi = new FileInfo(file);
                if (ImageCache.ContainsKey(fi.Name)) continue;
                AddImageToCache(fi.Name, fi.LastWriteTime, fi.Length);
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

        [JsonProperty("byteSize")]
        public long ByteSize { get; set; }
    }
}
