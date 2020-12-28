using System.Xml.Serialization;

namespace hdhr2mxf.XMLTV
{
    public class XmltvIcon
    {
        [XmlAttribute("src")]
        public string Src { get; set; }

        [XmlAttribute("width")]
        public string Width { get; set; }

        [XmlAttribute("height")]
        public string Height { get; set; }
    }
}
