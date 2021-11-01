using System.Xml.Serialization;

namespace epg123Client.SatMxf
{
    public class MxfDvbsService
    {
        [XmlIgnore] public MxfDvbsTransponder _transponder;
        [XmlIgnore] public int _serviceId;

        [XmlAttribute("uid")]
        public string Uid
        {
            get => $"{_transponder.Uid}!DvbsService[{_serviceId}]";
            set { }
        }

        [XmlAttribute("_name")]
        public string Name { get; set; }

        [XmlAttribute("_serviceId")]
        public int ServiceId
        {
            get => (short)(_serviceId & 0xFFFF);
            set => _serviceId = value;
        }
        public bool ShouldSerializeServiceId() => ServiceId != 0;

        [XmlAttribute("_serviceTypeValid")]
        public bool ServiceTypeValid
        {
            get => ServiceType >= 0 && ServiceType < 3;
            set { }
        }
        public bool ShouldSerializeServiceTypeValid() => ServiceTypeValid;

        [XmlAttribute("_serviceType")] // 0 = TV, 1 = Radio, 2 = Data
        public int ServiceType { get; set; }
        public bool ShouldSerializeServiceType() => ServiceType != 0;

        [XmlAttribute("_isEncryptedValid")]
        public bool IsEncryptedValid
        {
            get => true;
            set { }
        }

        [XmlAttribute("_isEncrypted")]
        public bool IsEncrypted { get; set; }
        public bool ShouldSerializeIsEncrypted() => IsEncrypted;
    }
}
