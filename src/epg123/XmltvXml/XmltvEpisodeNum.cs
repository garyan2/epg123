using System.Xml.Serialization;

namespace epg123.XmltvXml
{
    public class XmltvEpisodeNum
    {
        [XmlAttribute("system")]
        public string System { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
