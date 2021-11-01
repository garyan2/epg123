using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123Client.SatMxf
{
    public class MxfDvbsSatellite
    {
        [XmlAttribute("uid")]
        public string Uid
        {
            get => $"!DvbsDataSet!DvbsSatellite[{PositionEast}]";
            set { }
        }

        [XmlAttribute("_name")]
        public string Name { get; set; }

        [XmlAttribute("_positionEast")]
        public int PositionEast { get; set; }

        [XmlArrayItem("DvbsTransponder")]
        public List<MxfDvbsTransponder> _transponders { get; set; }
    }
}
