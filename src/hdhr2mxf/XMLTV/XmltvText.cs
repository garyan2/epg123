using System.Xml.Serialization;

namespace hdhr2mxf.XMLTV
{
    public class XmltvText
    {
        [XmlAttribute("lang")]
        public string Language { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
