using System.Xml.Serialization;

namespace epg123.XmltvXml
{
    public class XmltvIcon
    {
        [XmlAttribute("src")]
        public string src { get; set; }

        [XmlAttribute("width")]
        public int width { get; set; } = 0;
        public bool ShouldSerializewidth() { return width != 0; }

        [XmlAttribute("height")]
        public int height { get; set; } = 0;
        public bool ShouldSerializeheight() { return height != 0; }
    }
}
