using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    public class MxfDvbsDataSet
    {
        private string _uid;

        [XmlAttribute("uid")]
        public string Uid
        {
            get => _uid ?? "!DvbsDataSet";
            set { _uid = value; }
        }

        [XmlAttribute("_frequencyTolerance")]
        [DefaultValue(0)]
        public int FrequencyTolerance { get; set; } = 10;

        [XmlAttribute("_symbolRateTolerance")]
        [DefaultValue(0)]
        public int SymbolRateTolerance { get; set; } = 1500;

        [XmlAttribute("_minimumSearchMatches")]
        [DefaultValue(0)]
        public int MinimumSearchMatches { get; set; } = 3;

        [XmlAttribute("_dataSetRevision")]
        [DefaultValue(0)]
        public int DataSetRevision { get; set; }

        [XmlArrayItem("DvbsSatellite")]
        public List<MxfDvbsSatellite> _allSatellites { get; set; } = new List<MxfDvbsSatellite>();

        [XmlArrayItem("DvbsRegion")]
        public List<MxfDvbsRegion> _allRegions { get; set; } = new List<MxfDvbsRegion>();

        [XmlArrayItem("DvbsHeadend")]
        public List<MxfDvbsHeadend> _allHeadends { get; set; } = new List<MxfDvbsHeadend>();
    }
}
