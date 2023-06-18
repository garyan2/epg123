using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    public class MxfDvbsTransponder
    {
        private string _uid;

        [XmlIgnore] public MxfDvbsSatellite _satellite;
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

        public MxfDvbsService GetOrCreateService(string name, int sid, int type, bool encrypted)
        {
            var service = _services.SingleOrDefault(arg => arg.ServiceId == (short)(sid & 0xFFFF));
            if (service != null) return service;

            service = new MxfDvbsService
            {
                _transponder = this,
                Name = name,
                ServiceId = sid,
                ServiceType = type,
                IsEncrypted = encrypted
            };
            _services.Add(service);
            return service;
        }

        [XmlAttribute("uid")]
        public string Uid
        {
            get => _uid ?? $"{_satellite.Uid}!DvbsTransponder[{CarrierFrequency},{GetPolarizationString()},{SymbolRate}]";
            set { _uid = value; }
        }

        [XmlAttribute("_carrierFrequency")]
        [DefaultValue(0)]
        public int CarrierFrequency { get; set; }

        [XmlAttribute("_polarization")]
        [DefaultValue(0)]
        public int Polarization { get; set; }

        [XmlAttribute("_symbolRate")]
        [DefaultValue(0)]
        public int SymbolRate { get; set; }

        [XmlAttribute("_originalNetworkIdValid")]
        [DefaultValue(false)]
        public bool OriginalNetworkIdValid
        {
            get => OriginalNetworkId != 0;
            set { }
        }

        [XmlAttribute("_transportStreamIdValid")]
        [DefaultValue(false)]
        public bool TransportStreamIdValid
        {
            get => TransportStreamId != 0;
            set { }
        }

        [XmlAttribute("_originalNetworkId")]
        [DefaultValue(0)]
        public int OriginalNetworkId
        {
            get => (short)(_originalNetworkId & 0xFFFF);
            set => _originalNetworkId = value;
        }

        [XmlAttribute("_transportStreamId")]
        [DefaultValue(0)]
        public int TransportStreamId
        {
            get => (short)(_transportStreamId & 0xFFFF);
            set => _transportStreamId = value;
        }

        [XmlArrayItem("DvbsService")]
        public List<MxfDvbsService> _services { get; set; }
    }
}
