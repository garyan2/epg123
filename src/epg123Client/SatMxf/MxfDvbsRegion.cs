using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123Client.SatMxf
{
    public class MxfDvbsRegion
    {
        [XmlAttribute("uid")]
        public string Uid
        {
            get => $"!DvbsDataSet!DvbsRegion[{IsoCode}]";
            set { }
        }

        [XmlAttribute("_isoCode")]
        public string IsoCode { get; set; }

        [XmlArrayItem("DvbsFootprint")]
        public List<MxfDvbsFootprint> _footprints { get; set; }
    }
}
