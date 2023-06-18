using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    public class MxfSeries
    {
        [XmlAttribute("uid")]
        public string Uid { get; set; }

        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlAttribute("shortTitle")]
        public string ShortTitle { get; set; }

        [XmlAttribute("description")]
        public string DescriptionAttribute { get; set; }

        [XmlAttribute("shortDescription")]
        public string ShortDescriptionAttribute { get; set; }

        [XmlAttribute("studio")]
        public string Studio { get; set; }

        [XmlAttribute("startAirdate")]
        public string StartAirdate { get; set; }

        [XmlAttribute("endAirdate")]
        public string EndAirdate { get; set; }

        [XmlAttribute("year")]
        public string Year { get; set; }

        [XmlElement("description")]
        public string DescriptionElement { get; set; }

        [XmlElement("shortDescription")]
        public string ShortDescriptionElement { get; set; }
    }
}