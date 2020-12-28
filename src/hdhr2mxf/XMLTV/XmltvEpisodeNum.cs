using System.Xml.Serialization;

namespace hdhr2mxf.XMLTV
{
    public class XmltvEpisodeNum
    {
        [XmlAttribute("system")]
        public string System { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
