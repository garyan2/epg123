using System.ComponentModel;
using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    public class MxfDvbsService
    {
        private string _uid;

        [XmlIgnore] public MxfDvbsTransponder _transponder;
        [XmlIgnore] public int _serviceId;

        [XmlAttribute("uid")]
        public string Uid
        {
            get => _uid ?? $"{_transponder.Uid}!DvbsService[{_serviceId}]";
            set { _uid = value; }
        }

        [XmlAttribute("_name")]
        public string Name { get; set; }

        [XmlAttribute("_serviceId")]
        [DefaultValue(0)]
        public int ServiceId
        {
            get => (short)(_serviceId & 0xFFFF);
            set => _serviceId = value;
        }

        [XmlAttribute("_serviceTypeValid")]
        [DefaultValue(false)]
        public bool ServiceTypeValid
        {
            get => ServiceType >= 0 && ServiceType < 3;
            set { }
        }

        [XmlAttribute("_serviceType")] // 0 = TV, 1 = Radio, 2 = Data
        [DefaultValue(0)]
        public int ServiceType { get; set; }

        [XmlAttribute("_isEncryptedValid")]
        [DefaultValue(false)]
        public bool IsEncryptedValid
        {
            get => true;
            set { }
        }

        [XmlAttribute("_isEncrypted")]
        [DefaultValue(false)]
        public bool IsEncrypted { get; set; }
    }
}
