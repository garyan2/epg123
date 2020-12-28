using System.Collections.Generic;
using System.Xml.Serialization;

namespace hdhr2mxf.MXF
{
    public class MxfService
    {
        [XmlIgnore]
        public int Index;

        [XmlIgnore]
        public bool IsHd;

        [XmlIgnore]
        public MxfScheduleEntries MxfScheduleEntries = new MxfScheduleEntries() { ScheduleEntry = new List<MxfScheduleEntry>() };

        [XmlIgnore]
        public string StationId { get; set; }

        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// Use IDs such as s1, s2, s3, and so forth.
        /// </summary>
        [XmlAttribute("id")]
        public string Id
        {
            get => ("s" + Index.ToString());
            set { }
        }

        /// <summary>
        /// An ID that uniquely identifies the service.
        /// Should be of the form "!Service!name", where name is the value of the name attribute.
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get => ("!Service!EPG123_" + StationId);
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
        public string Affiliate { get; set; }

        /// <summary>
        /// Specifies a logo image to display.
        /// This value contains a GuideImage id attribute. When searching for a logo to display, the service is searched first, and then its affiliate.
        /// </summary>
        [XmlAttribute("logoImage")]
        public string LogoImage { get; set; }
    }
}