using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    public partial class MXF
    {
        public MxfDvbsRegion GetOrCreateRegion(string isoCode)
        {
            var region = DvbsDataSet._allRegions.SingleOrDefault(arg => arg.IsoCode == isoCode);
            if (region != null) return region;

            region = new MxfDvbsRegion
            {
                IsoCode = isoCode,
                _footprints = new List<MxfDvbsFootprint>()
            };
            DvbsDataSet._allRegions.Add(region);
            return region;
        }
    }

    public class MxfDvbsRegion
    {
        private string _uid;

        public MxfDvbsFootprint GetOrCreateFootprint(MxfDvbsSatellite satellite)
        {
            var footprint = _footprints.SingleOrDefault(arg => arg._mxfSatellite == satellite);
            if (footprint != null) return footprint;

            footprint = new MxfDvbsFootprint
            {
                _mxfRegion = this,
                _mxfSatellite = satellite,
                headends = new List<MxfDvbsHeadend>()
            };
            _footprints.Add(footprint);
            return footprint;
        }

        [XmlAttribute("uid")]
        public string Uid
        {
            get => _uid ?? $"!DvbsDataSet!DvbsRegion[{IsoCode}]";
            set { _uid = value; }
        }

        [XmlAttribute("_isoCode")]
        public string IsoCode { get; set; }

        [XmlArrayItem("DvbsFootprint")]
        public List<MxfDvbsFootprint> _footprints { get; set; }
    }
}
