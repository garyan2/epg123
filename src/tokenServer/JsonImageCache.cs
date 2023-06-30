using GaRyan2.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace tokenServer
{
    public static class JsonImageCache
    {
        public static Dictionary<string, CacheImage> ImageCache = new Dictionary<string, CacheImage>();
        private static readonly object _cacheLock = new object();
        public static bool cacheImages => cacheRetention > 0;
        public static int cacheRetention;

        public static void Initialize()
        {
            if (File.Exists(Helper.Epg123ImageCachePath))
            {
                ImageCache = Helper.ReadJsonFile(Helper.Epg123ImageCachePath, typeof(Dictionary<string, CacheImage>));
            }
            else ImageCache = new Dictionary<string, CacheImage>();

            GetAllImageSizes();
            AddImagesMissingInCacheFile();
        }

        public static void Cleanup()
        {
            if (!cacheImages) return;
            var markedForDelete = ImageCache.Where(arg => arg.Value.LastUsed + TimeSpan.FromDays(cacheRetention) < DateTime.Now)
                .Select(arg => arg.Key).ToList();
            foreach (var image in markedForDelete)
            {
                var path = $"{Helper.Epg123ImageCache}{image.Substring(0, 1)}\\{image}";
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
                Helper.WriteJsonFile(ImageCache, Helper.Epg123ImageCachePath);
            }
        }

        public static FileInfo GetCachedImage(string filename)
        {
            // remove "/image/" from beginning
            filename = filename.Substring(7);
            var location = $"{Helper.Epg123ImageCache}{filename.Substring(0, 1)}\\{filename}";
            lock (_cacheLock)
            {
                var fi = new FileInfo(location);
                if (ImageCache.ContainsKey(filename))
                {
                    if (fi.Exists)
                    {
                        if (DateTime.Now - ImageCache[filename].LastUsed > TimeSpan.FromHours(24)) ImageCache[filename].LastUsed = DateTime.Now;
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

        public static void AddImageToCache(string filename, DateTimeOffset lastModified, long size)
        {
            lock (_cacheLock)
            {
                if (ImageCache.ContainsKey(filename))
                {
                    ImageCache[filename].LastUsed = DateTime.Now;
                    ImageCache[filename].LastModified = lastModified.LocalDateTime;
                    ImageCache[filename].ByteSize = size;
                }
                else ImageCache.Add(filename, new CacheImage { LastUsed = DateTime.Now, LastModified = lastModified.LocalDateTime, ByteSize = size});
            }
        }

        public static void GetAllImageSizes()
        {
            foreach (var image in ImageCache.Where(arg => arg.Value.ByteSize == 0).ToList())
            {
                var fi = new FileInfo($"{Helper.Epg123ImageCache}{image.Key.Substring(0, 1)}\\{image.Key}");
                if (fi.Exists)
                {
                    image.Value.ByteSize = fi.Length;
                }
            }
        }

        public static void AddImagesMissingInCacheFile()
        {
            var files = Directory.GetFiles($"{Helper.Epg123ImageCache}", "*.*", SearchOption.AllDirectories);
            foreach (var file in files.Where(arg => arg.EndsWith("jpg") || arg.EndsWith("png")).ToList())
            {
                var fi = new FileInfo(file);
                if (ImageCache.ContainsKey(fi.Name)) continue;
                AddImageToCache(fi.Name, fi.LastWriteTime, fi.Length);
            }
        }

        public static bool IsImageRecent(string filename, DateTimeOffset ifModifiedSince)
        {
            lock (_cacheLock)
            {
                if (!ImageCache.ContainsKey(filename)) return false;
                if (ImageCache[filename].LastModified == DateTime.MinValue)
                {
                    var info = new FileInfo($"{Helper.Epg123ImageCache}{filename.Substring(0, 1)}\\{filename}");
                    ImageCache[filename].LastModified = info.LastWriteTime;
                }
                if (ifModifiedSince == DateTime.MinValue && DateTime.Now - ImageCache[filename].LastModified < TimeSpan.FromDays(30))
                {
                    return true;
                }
                if (ImageCache[filename].LastUsed + TimeSpan.FromHours(24) > DateTime.Now) return true;
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
