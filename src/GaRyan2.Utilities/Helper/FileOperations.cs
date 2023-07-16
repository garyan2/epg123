using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Serialization;

namespace GaRyan2.Utilities
{
    public static partial class Helper
    {
        public static bool WriteXmlFile(object obj, string filepath, bool compress = false)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filepath));
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                var ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                using (var writer = new StreamWriter(filepath, false, Encoding.UTF8))
                {
                    serializer.Serialize(writer, obj, ns);
                }
                if (compress && InstallMethod != Installation.PORTABLE)
                {
                    GZipCompressFile(filepath);
                    DeflateCompressFile(filepath);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Failed to write file \"{filepath}\"; {ex}");
            }
            return false;
        }

        public static dynamic ReadXmlFile(string filepath, Type type)
        {
            if (!File.Exists(filepath))
            {
                //Logger.WriteInformation($"File \"{filepath}\" does not exist.");
                return null;
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(type);
                using (var reader = new StreamReader(filepath, Encoding.Default))
                {
                    return serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Failed to read file \"{filepath}\"; {ex}");
            }
            return null;
        }

        public static bool WriteJsonFile(object obj, string filepath)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filepath));
                using (var writer = File.CreateText(filepath))
                {
                    var serializer = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.None };
                    serializer.Serialize(writer, obj);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Failed to write file \"{filepath}\"; {ex}");
            }
            return false;
        }

        public static dynamic ReadJsonFile(string filepath, Type type)
        {
            if (!File.Exists(filepath))
            {
                //Logger.WriteInformation($"File \"{filepath}\" does not exist.");
                return null;
            }

            try
            {
                using (var file = File.OpenText(filepath))
                using (var reader = new JsonTextReader(file))
                {
                    var serializer = new JsonSerializer();
                    return serializer.Deserialize(reader, type);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Failed to read file \"{filepath}\"; {ex}");
            }
            return null;
        }

        public static bool DeleteFile(string filepath)
        {
            if (!File.Exists(filepath)) return true;
            try
            {
                File.Delete(filepath);
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteInformation($"Failed to delete file \"{filepath}\"; {ex}");
            }
            return false;
        }

        public static FileInfo GZipCompressFile(string filepath)
        {
            var fileToCompress = new FileInfo(filepath);
            using (var originalFileStream = fileToCompress.OpenRead())
            using (var compressedFileStream = File.Create(fileToCompress.FullName + ".gz"))
            using (var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
            {
                originalFileStream.CopyTo(compressionStream);
            }
            return new FileInfo(fileToCompress.FullName + ".gz");
        }

        public static FileInfo DeflateCompressFile(string filepath)
        {
            var fileToCompress = new FileInfo(filepath);
            using (var originalFileStream = fileToCompress.OpenRead())
            using (var compressedFileStream = File.Create(fileToCompress.FullName + ".zz"))
            using (var compressionStream = new DeflateStream(compressedFileStream, CompressionMode.Compress))
            {
                originalFileStream.CopyTo(compressionStream);
            }
            return new FileInfo(fileToCompress.FullName + ".zz");
        }
    }
}