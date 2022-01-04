using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace tokenServer
{
    public static class Config
    {
        public static epgConfig GetEpgConfig()
        {
            if (!File.Exists(Helper.Epg123CfgPath)) return null;
            using (var stream = new StreamReader(Helper.Epg123CfgPath, Encoding.Default))
            {
                var serializer = new XmlSerializer(typeof(epgConfig));
                TextReader reader = new StringReader(stream.ReadToEnd());
                return (epgConfig)serializer.Deserialize(reader);
            }
        }

        public static string GetXmltvPath()
        {
            var config = GetEpgConfig();
            return config?.XmltvOutputFile ?? Helper.Epg123XmltvPath;
        }
    }

    [XmlRoot("EPG123")]
    public class epgConfig
    {
        [XmlElement("UserAccount")]
        public SdUserAccount UserAccount { get; set; }

        [XmlElement("XmltvOutputFile")]
        public string XmltvOutputFile { get; set; } = Helper.Epg123XmltvPath;
    }

    public class SdUserAccount
    {
        [XmlElement("LoginName")]
        public string LoginName { get; set; }

        [XmlElement("PasswordHash")]
        public string PasswordHash { get; set; }
    }
}