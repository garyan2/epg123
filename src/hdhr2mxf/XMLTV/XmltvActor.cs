using System.Xml.Serialization;

namespace hdhr2mxf.XMLTV
{
    public class XmltvActor
    {
        [XmlAttribute("role")]
        public string Role { get; set; }

        [XmlText]
        public string Actor { get; set; }
    }
}
