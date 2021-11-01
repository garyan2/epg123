using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123Client.SatMxf
{
    public class MxfDvbsDataSet
    {
        [XmlAttribute("uid")]
        public string Uid
        {
            get => "!DvbsDataSet";
            set { }
        }

        [XmlAttribute("_frequencyTolerance")]
        public int FrequencyTolerance { get; set; } = 10;
        public bool ShouldSerializeFrequencyTolerance() => FrequencyTolerance != 0;

        [XmlAttribute("_symbolRateTolerance")]
        public int SymbolRateTolerance { get; set; } = 1500;
        public bool ShouldSerializeSymbolRateTolerance() => SymbolRateTolerance != 0;

        [XmlAttribute("_minimumSearchMatches")]
        public int MinimumSearchMatches { get; set; } = 3;
        public bool ShouldSerializeMinimumSearchMatches() => MinimumSearchMatches != 0;

        [XmlAttribute("_dataSetRevision")]
        public int DataSetRevision { get; set; }
        public bool ShouldSerializeDataSetRevision() => DataSetRevision != 0;

        [XmlArrayItem("DvbsSatellite")]
        public List<MxfDvbsSatellite> _allSatellites { get; set; } = new List<MxfDvbsSatellite>();

        [XmlArrayItem("DvbsRegion")]
        public List<MxfDvbsRegion> _allRegions { get; set; } = new List<MxfDvbsRegion>();

        [XmlArrayItem("DvbsHeadend")]
        public List<MxfDvbsHeadend> _allHeadends { get; set; } = new List<MxfDvbsHeadend>();
    }
}
