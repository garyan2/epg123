using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;

namespace epg123
{
    class CompressXmlFiles
    {
        public bool CompressSingleStreamToFile(Stream stream, string fileUri, string filePath,
            CompressionOption option = CompressionOption.Normal)
        {
            using (var pack = Package.Open(filePath, FileMode.Create))
            {
                var part = pack.CreatePart(new Uri(fileUri, UriKind.Relative),
                    System.Net.Mime.MediaTypeNames.Text.Xml, option);
                if (part != null) CopyStream(stream, part.GetStream());
            }

            return true;
        }

        /// <summary>
        /// Creates a compressed zip file in a subfolder called "backup"
        /// </summary>
        /// <param name="files">KeyValuePair is FilePath, ArchivePath</param>
        /// <param name="archivePrefix">string that will prepend the archive filename</param>
        public string CreatePackage(Dictionary<string, string> files, string archivePrefix)
        {
            // build the filepath for the destination archive
            var filepath = Helper.Epg123BackupFolder;
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }

            filepath += "\\" + archivePrefix + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".zip";

            // create the zip and add the file(s)
            using (var pack = Package.Open(filepath, FileMode.Create))
            {
                foreach (var file in files)
                {
                    var part = pack.CreatePart(PackUriHelper.CreatePartUri(new Uri(file.Value, UriKind.Relative)),
                        System.Net.Mime.MediaTypeNames.Text.Xml, CompressionOption.Maximum);
                    using (var fileStream = new FileStream(file.Key, FileMode.Open, FileAccess.Read))
                    {
                        if (part != null) CopyStream(fileStream, part.GetStream());
                    }
                }
            }

            return filepath;
        }

        private static Package package;

        public Stream GetBackupFileStream(string backup, string fileUri = null)
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

            return package != null ? (from part in package.GetParts() where part.Uri.ToString().Contains(backup) select part.GetStream()).FirstOrDefault() : null;
        }

        public void ClosePackage()
        {
            package?.Close();
        }

        private void CopyStream(Stream source, Stream target)
        {
            const int bufSize = 0x1000;
            var buf = new byte[bufSize];
            int bytesRead;
            while ((bytesRead = source.Read(buf, 0, bufSize)) > 0)
            {
                target.Write(buf, 0, bytesRead);
            }
        }
    }
}