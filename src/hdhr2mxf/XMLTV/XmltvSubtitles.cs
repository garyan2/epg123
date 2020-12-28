using System.Xml.Serialization;

namespace hdhr2mxf.XMLTV
{
    public class XmltvSubtitles
    {
        [XmlElement("language")]
        public string Language { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }
    }
}
