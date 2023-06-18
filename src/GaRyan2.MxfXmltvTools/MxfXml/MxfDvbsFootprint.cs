using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    public class MxfDvbsFootprint
    {
        private string _uid;
        private string _satellite;

        [XmlIgnore] public MxfDvbsRegion _mxfRegion;
        [XmlIgnore] public MxfDvbsSatellite _mxfSatellite;

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
            get => _uid ?? $"!DvbsDataSet!DvbsFootprint[{_mxfRegion.IsoCode}:{_mxfSatellite.PositionEast}]";
            set { _uid = value; }
        }

        [XmlAttribute("_satellite")]
        public string Satellite
        {
            get => _satellite ?? _mxfSatellite.Uid;
            set { _satellite = value; }
        }

        [XmlAttribute("referenceService")]
        public string ReferenceService { get; set; }

        [XmlArrayItem("DvbsHeadend")]
        public List<MxfDvbsHeadend> headends { get; set; }
    }
}
