using System.Xml.Serialization;

namespace hdhr2mxf.XMLTV
{
    public class XmltvLength
    {
        [XmlAttribute("units")]
        public string Units { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
