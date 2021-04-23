using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123.MxfXml
{
    public partial class Mxf
    {
        private readonly Dictionary<string, MxfService> _services = new Dictionary<string, MxfService>();
        public MxfService GetService(string stationId)
        {
            if (_services.TryGetValue(stationId, out var service)) return service;
            With.Services.Add(service = new MxfService
            {
                Index = With.Services.Count + 1,
                StationId = stationId
            });
            service.MxfScheduleEntries.Service = service.Id;
            With.ScheduleEntries.Add(service.MxfScheduleEntries);
            _services.Add(stationId, service);
            return service;
        }
    }

    public class MxfService
    {
        public override string ToString() { return Id; }

        [XmlIgnore] public int Index;
        [XmlIgnore] public string StationId;
        [XmlIgnore] public string UidOverride;
        [XmlIgnore] public MxfAffiliate mxfAffiliate;
        [XmlIgnore] public MxfGuideImage mxfGuideImage;
        [XmlIgnore] public MxfScheduleEntries MxfScheduleEntries = new MxfScheduleEntries { ScheduleEntry = new List<MxfScheduleEntry>() };

        [XmlIgnore] public Dictionary<string, dynamic> extras = new Dictionary<string, dynamic>();

        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// Use IDs such as s1, s2, s3, and so forth.
        /// </summary>
        [XmlAttribute("id")]
        public string Id
        {
            get => $"s{Index}";
            set { }
        }

        /// <summary>
        /// An ID that uniquely identifies the service.
        /// Should be of the form "!Service!name", where name is the value of the name attribute.
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get => string.IsNullOrEmpty(UidOverride) ? $"!Service!{StationId}" : $"!Service!{UidOverride}";
            set { }
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
            get => mxfAffiliate?.ToString();
            set { }
        }

        /// <summary>
        /// Specifies a logo image to display.
        /// This value contains a GuideImage id attribute. When searching for a logo to display, the service is searched first, and then its affiliate.
        /// </summary>
        [XmlAttribute("logoImage")]
        public string LogoImage
        {
            get => mxfGuideImage?.ToString();
            set { }
        }
    }
}