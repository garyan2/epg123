using System.Xml.Serialization;

namespace epg123.XmltvXml
{
    public class XmltvText
    {
        [XmlAttribute("lang")]
        public string Language { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
