using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace tokenServer
{
    public class JsonImageCache
    {
        public Dictionary<string, CacheImage> ImageCache;
        private readonly object _cacheLock = new object();
        public bool cacheImages;
        public int cacheRetention;

        public JsonImageCache()
        {
            Load();
        }

        private void Load()
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

        public void Cleanup()
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

        public void Save()
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

        public FileInfo GetCachedImage(string filename)
        {
            lock (_cacheLock)
            {
                // remove "/image/" from beginning
                filename = filename.Substring(7);
                var location = $"{Helper.Epg123ImageCache}\\{filename.Substring(0, 1)}\\{filename}";
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

        public FileInfo SaveImageToCache(string filename, Stream stream, DateTime lastModified)
        {
            lock (_cacheLock)
            {
                // remove "/image/" from beginning
                filename = filename.Substring(7);
                try
                {
                    var image = Image.FromStream(stream);
                    var baseFolder = $"{Helper.Epg123ImageCache}\\{filename.Substring(0, 1)}";
                    var location = $"{baseFolder}\\{filename}";
                    _ = Directory.CreateDirectory(baseFolder);
                    image.Save(location);
                    if (lastModified != DateTime.MinValue) File.SetLastWriteTimeUtc(location, lastModified);
                    if (ImageCache.ContainsKey(filename)) ImageCache[filename].LastUsed = DateTime.Now;
                    else ImageCache.Add(filename, new CacheImage { LastUsed = DateTime.Now });
                    return new FileInfo(location);
                }
                catch { }
                return null;
            }
        }
    }

    public class CacheImage
    {
        [JsonProperty("LastUsed")]
        public DateTime LastUsed { get; set; }
    }
}
