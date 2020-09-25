using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace epg123
{
    public static partial class sdJson2mxf
    {
        private static archiveImageLibrary newImageLibrary = new archiveImageLibrary() { Images = new List<archiveImage>() };
        private static archiveImageLibrary oldImageLibrary = new archiveImageLibrary() { Images = new List<archiveImage>() };

        private static void getImageArchive()
        {
            newImageLibrary = new archiveImageLibrary()
            {
                Version = epg123Version,
                Images = new List<archiveImage>()
            };

            // check that file exists
            if (!File.Exists(Helper.Epg123GuideImagesXmlPath)) return;

            try
            {
                using (StreamReader stream = new StreamReader(Helper.Epg123GuideImagesXmlPath, Encoding.Default))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(archiveImageLibrary));
                    TextReader reader = new StringReader(stream.ReadToEnd());
                    oldImageLibrary = (archiveImageLibrary)serializer.Deserialize(reader);
                    reader.Close();

                    //foreach (archiveImage image in old_imgs.Images)
                    //{
                    //    // if url is still pointing to json.schedulesdirect.org, do not include in old library array
                    //    // an empty url is a movie with no cover, do not include to avoid high number of calls to tmdb
                    //    if (string.IsNullOrEmpty(image.Url) || !image.Url.ToLower().Contains("json.schedulesdirect.org"))
                    //    {
                    //        oldImageLibrary.Add(image.Zap2itId, image.Url);
                    //    }
                    //}
                }
            }
            catch (IOException ioe)
            {
                Logger.WriteError(string.Format("IOException occurred. Message: {0}. Exiting.", ioe.Message));
            }
            catch (Exception e)
            {
                Logger.WriteError(string.Format("Unknown exception occurred. Message: {0}. Exiting.", e.Message));
            }
        }

        private static void writeImageArchive()
        {
            try
            {
                using (StreamWriter stream = new StreamWriter(Helper.Epg123GuideImagesXmlPath, false, Encoding.UTF8))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(archiveImageLibrary));
                    TextWriter writer = stream;
                    serializer.Serialize(writer, newImageLibrary);
                }

                Logger.WriteInformation(string.Format("Completed save of image archive file to \"{0}\".", Helper.Epg123GuideImagesXmlPath));
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("Failed to save the image archive file to \"{0}\". Message: {1}", Helper.Epg123GuideImagesXmlPath, ex.Message));
            }
        }
    }

    [XmlRoot("archive")]
    public class archiveImageLibrary
    {
        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlArrayItem("Image")]
        public List<archiveImage> Images { get; set; }
    }

    public class archiveImage
    {
        [XmlAttribute("zap2itId")]
        public string Zap2itId { get; set; }

        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlAttribute("url")]
        public string Url { get; set; }

        [XmlAttribute("width")]
        public int Width { get; set; }

        [XmlAttribute("height")]
        public int Height { get; set; }
    }
}