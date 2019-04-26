using System.Xml.Serialization;

namespace epg123
{
    [XmlRoot("Channels")]
    public class colossusChannels
    {
        [XmlElement("Lineup")]
        public colossusLineup Colossus { get; set; }
    }

    public class colossusLineup
    {
        [XmlAttribute("epg123")]
        public bool epg123 { get; set; }

        [XmlText()]
        public string Lineup { get; set; }
    }
}
