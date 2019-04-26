using System;
using System.Xml.Serialization;

namespace epg123.XmltvXml
{
    public class XmltvEpisodeNum
    {
        [XmlAttribute("system")]
        public String System { get; set; }

        [XmlText]
        public String Text { get; set; }
    }
}
