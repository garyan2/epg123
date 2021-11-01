using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123Client.SatMxf
{
    public class MxfDvbsFootprint
    {
        [XmlIgnore] public MxfDvbsRegion _region;
        [XmlIgnore] public MxfDvbsSatellite _satellite;

        [XmlAttribute("uid")]
        public string Uid
        {
            get => $"!DvbsDataSet!DvbsFootprint[{_region.IsoCode}:{_satellite.PositionEast}]";
            set { }
        }

        [XmlAttribute("_satellite")]
        public string Satellite
        {
            get => _satellite.Uid;
            set { }
        }

        [XmlAttribute("referenceService")]
        public string ReferenceService { get; set; }

        [XmlArrayItem("DvbsHeadend")]
        public List<MxfDvbsHeadend> headends { get; set; }
    }
}
