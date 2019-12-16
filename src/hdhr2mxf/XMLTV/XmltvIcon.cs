using System.Xml.Serialization;

namespace XmltvXml
{
    public class XmltvIcon
    {
        [XmlAttribute("src")]
        public string src { get; set; }

        [XmlAttribute("width")]
        public string width { get; set; }

        [XmlAttribute("height")]
        public string height { get; set; }
    }
}
