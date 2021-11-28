using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace epg123Client.SatMxf
{
    public partial class Mxf
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
        public MxfDvbsFootprint GetOrCreateFootprint(MxfDvbsSatellite satellite)
        {
            var footprint = _footprints.SingleOrDefault(arg => arg._satellite == satellite);
            if (footprint != null) return footprint;

            footprint = new MxfDvbsFootprint
            {
                _region = this,
                _satellite = satellite,
                headends = new List<MxfDvbsHeadend>()
            };
            _footprints.Add(footprint);
            return footprint;
        }

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
