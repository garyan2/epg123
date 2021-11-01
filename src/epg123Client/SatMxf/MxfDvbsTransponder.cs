using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123Client.SatMxf
{
    public class MxfDvbsTransponder
    {
        [XmlIgnore] public MxfDvbsSatellite _satellite;
        [XmlIgnore] public bool IncludeInHeadend;
        [XmlIgnore] public int _originalNetworkId;
        [XmlIgnore] public int _transportStreamId;

        private string GetPolarizationString()
        {
            switch (Polarization)
            {
                case 0:
                    return "LinearHorizontal";
                case 1:
                    return "LinearVertical";
                case 2:
                    return "CircularLeft";
                case 3:
                default:
                    return "CircularRight";
            }
        }

        [XmlAttribute("uid")]
        public string Uid
        {
            get => $"{_satellite.Uid}!DvbsTransponder[{CarrierFrequency},{GetPolarizationString()},{SymbolRate}]";
            set { }
        }

        [XmlAttribute("_carrierFrequency")]
        public int CarrierFrequency { get; set; }
        public bool ShouldSerializeCarrierFrequency() => CarrierFrequency != 0;

        [XmlAttribute("_polarization")]
        public int Polarization { get; set; }
        public bool ShouldSerializePolarization() => Polarization != 0;

        [XmlAttribute("_symbolRate")]
        public int SymbolRate { get; set; }
        public bool ShouldSerializeSymbolRate() => SymbolRate != 0;

        [XmlAttribute("_originalNetworkIdValid")]
        public bool OriginalNetworkIdValid
        {
            get => OriginalNetworkId != 0;
            set { }
        }
        public bool ShouldSerializeOriginalNetworkIdValid() => OriginalNetworkIdValid;

        [XmlAttribute("_transportStreamIdValid")]
        public bool TransportStreamIdValid
        {
            get => TransportStreamId != 0;
            set { }
        }
        public bool ShouldSerializeTransportStreamIdValid() => TransportStreamIdValid;

        [XmlAttribute("_originalNetworkId")]
        public int OriginalNetworkId
        {
            get => (short)(_originalNetworkId & 0xFFFF);
            set => _originalNetworkId = value;
        }
        public bool ShouldSerializeOriginalNetworkId() => OriginalNetworkId != 0;

        [XmlAttribute("_transportStreamId")]
        public int TransportStreamId
        {
            get => (short)(_transportStreamId & 0xFFFF);
            set => _transportStreamId = value;
        }
        public bool ShouldSerializeTransportStreamId() => TransportStreamId != 0;

        [XmlArrayItem("DvbsService")]
        public List<MxfDvbsService> _services { get; set; }
    }
}
