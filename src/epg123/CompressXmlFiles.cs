using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;

namespace epg123
{
    class CompressXmlFiles
    {
        public static bool CompressSingleStreamToFile(Stream stream, string fileUri, string filePath, CompressionOption option = CompressionOption.Normal)
        {
            using (var pack = ZipPackage.Open(filePath, FileMode.Create))
            {
                var part = pack.CreatePart(new Uri(fileUri, UriKind.Relative), 
                    System.Net.Mime.MediaTypeNames.Text.Xml, option);
                CopyStream(stream, part.GetStream());
            }
            return true;
        }

        /// <summary>
        /// Creates a compressed zip file in a subfolder called "backup"
        /// </summary>
        /// <param name="files">KeyValuePair is FilePath, ArchivePath</param>
        /// <param name="archivePrefix">string that will prepend the archive filename</param>
        public static string CreatePackage(Dictionary<string, string> files, string archivePrefix)
        {
            // build the filepath for the destination archive
            string filepath = Helper.Epg123BackupFolder;
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
            filepath += "\\" + archivePrefix + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".zip";

            // create the zip and add the file(s)
            using (Package package = Package.Open(filepath, FileMode.Create))
            {
                foreach (KeyValuePair<string, string> file in files)
                {
                    PackagePart part = package.CreatePart(PackUriHelper.CreatePartUri(new Uri(file.Value, UriKind.Relative)),
                                                          System.Net.Mime.MediaTypeNames.Text.Xml, CompressionOption.Maximum);
                    using (FileStream fileStream = new FileStream(file.Key, FileMode.Open, FileAccess.Read))
                    {
                        CopyStream(fileStream, part.GetStream());
                    }
                }
            }
            return filepath;
        }

        private static Package package = null;
        public static Stream GetBackupFileStream(string backup, string fileUri = null)
        {
            if (!string.IsNullOrEmpty(fileUri))
            {
                if (package != null)
                {
                    package.Close();
                    package = null;
                }
                package = Package.Open(fileUri, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (package != null)
            {
                foreach (PackagePart part in package.GetParts())
                {
                    if (part.Uri.ToString().Contains(backup))
                    {
                        return part.GetStream();
                    }
                }
            }
            return null;
        }

        public static void ClosePackage()
        {
            if (package != null)
            {
                package.Close();
            }
        }

        private static void CopyStream(Stream source, Stream target)
        {
            const int bufSize = 0x1000;
            byte[] buf = new byte[bufSize];
            int bytesRead = 0;
            while ((bytesRead = source.Read(buf, 0, bufSize)) > 0)
            {
                target.Write(buf, 0, bytesRead);
            }
        }
    }
}
