using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123.XmltvXml
{
    public class XmltvChannel
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlElement("display-name")]
        public List<XmltvText> DisplayNames { get; set; }

        [XmlElement("icon")]
        public List<XmltvIcon> Icons { get; set; }

        [XmlElement("url")]
        public List<string> Urls { get; set; }
    }
}
