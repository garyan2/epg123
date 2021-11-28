using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace epg123Client.SatMxf
{
    public class MxfDvbsFootprint
    {
        [XmlIgnore] public MxfDvbsRegion _region;
        [XmlIgnore] public MxfDvbsSatellite _satellite;

        public MxfDvbsHeadend GetOrCreateHeadend(int csiId)
        {
            var headend = headends.SingleOrDefault(arg => arg.CsiId == csiId);
            if (headend != null) return headend;

            headend = new MxfDvbsHeadend
            {
                CsiId = csiId,
                _channels = new List<MxfDvbsChannel>()
            };
            headends.Add(headend);
            return headend;
        }

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
