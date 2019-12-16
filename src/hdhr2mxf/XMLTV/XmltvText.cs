using System.Xml.Serialization;

namespace XmltvXml
{
    public class XmltvText
    {
        [XmlAttribute("lang")]
        public string Language { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
