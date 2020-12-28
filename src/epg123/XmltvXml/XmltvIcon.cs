using System.Xml.Serialization;

namespace epg123.XmltvXml
{
    public class XmltvIcon
    {
        [XmlAttribute("src")]
        public string Src { get; set; }

        [XmlAttribute("width")]
        public int Width { get; set; }
        public bool ShouldSerializeWidth() { return Width != 0; }

        [XmlAttribute("height")]
        public int Height { get; set; }
        public bool ShouldSerializeHeight() { return Height != 0; }
    }
}
