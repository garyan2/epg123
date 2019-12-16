using System.Xml.Serialization;

namespace XmltvXml
{
    public class XmltvPreviouslyShown
    {
        [XmlAttribute("start")]
        public string Start { get; set; }

        [XmlAttribute("channel")]
        public string Channel { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
