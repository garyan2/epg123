using System.Xml.Serialization;

namespace epg123.XmltvXml
{
    public class XmltvActor
    {
        [XmlAttribute("role")]
        public string Role { get; set; }

        [XmlText]
        public string Actor { get; set; }
    }
}
