using System.Collections.Generic;
using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    public partial class MXF
    {
        [XmlIgnore] public List<MxfService> ServicesToProcess = new List<MxfService>();

        private readonly Dictionary<string, MxfService> _services = new Dictionary<string, MxfService>();
        public MxfService FindOrCreateService(string stationId)
        {
            if (_services.TryGetValue(stationId, out var service)) return service;
            With.Services.Add(service = new MxfService(With.Services.Count + 1, stationId));
            With.ScheduleEntries.Add(service.MxfScheduleEntries);
            _services.Add(stationId, service);
            ServicesToProcess.Add(service);
            return service;
        }
    }

    public class MxfService
    {
        public string StationId => _stationId;

        private int _index;
        private string _affiliate;
        private string _logoImage;
        private readonly string _stationId;

        [XmlIgnore] public string UidOverride;
        [XmlIgnore] public MxfAffiliate mxfAffiliate;
        [XmlIgnore] public MxfGuideImage mxfGuideImage;
        [XmlIgnore] public MxfScheduleEntries MxfScheduleEntries;

        [XmlIgnore] public Dictionary<string, dynamic> extras = new Dictionary<string, dynamic>();

        public MxfService(int index, string stationId)
        {
            _index = index;
            _stationId = stationId;
            MxfScheduleEntries = new MxfScheduleEntries() { Service = Id, ScheduleEntry = new List<MxfScheduleEntry>() };
        }
        private MxfService() { }

        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// Use IDs such as s1, s2, s3, and so forth.
        /// </summary>
        [XmlAttribute("id")]
        public string Id
        {
            get => $"s{_index}";
            set { _index = int.Parse(value.Substring(1)); }
        }

        /// <summary>
        /// An ID that uniquely identifies the service.
        /// Should be of the form "!Service!name", where name is the value of the name attribute.
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get => UidOverride ?? $"!Service!{_stationId}";
            set { UidOverride = value; }
        }

        /// <summary>
        /// The display name of the service.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// The call sign of the service.
        /// For example, "BBC1".
        /// </summary>
        [XmlAttribute("callSign")]
        public string CallSign { get; set; }

        /// <summary>
        /// The ID of an Affiliate element that to which this service is affiliated.
        /// </summary>
        [XmlAttribute("affiliate")]
        public string Affiliate
        {
            get => _affiliate ?? mxfAffiliate?.Uid;
            set { _affiliate = value; }
        }

        /// <summary>
        /// Specifies a logo image to display.
        /// This value contains a GuideImage id attribute. When searching for a logo to display, the service is searched first, and then its affiliate.
        /// </summary>
        [XmlAttribute("logoImage")]
        public string LogoImage
        {
            get => _logoImage ?? mxfGuideImage?.Id ?? "";
            set { _logoImage = value; }
        }
    }
}