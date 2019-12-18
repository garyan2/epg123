using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123.MxfXml
{
    public class MxfService
    {
        [XmlIgnore]
        public int index;

        [XmlIgnore]
        public MxfScheduleEntries mxfScheduleEntries = new MxfScheduleEntries() { ScheduleEntry = new List<MxfScheduleEntry>() };

        [XmlIgnore]
        public string StationID { get; set; }

        [XmlIgnore]
        public SdStationImage logoImage;

        [XmlIgnore]
        public string xmltvChannelID
        {
            get
            {
                if (!string.IsNullOrEmpty(StationID))
                {
                    return "EPG123." + StationID + ".schedulesdirect.org";
                }
                return null;
            }
        }

        /// <summary>
        /// A content provider.
        /// Example: KOMO
        /// </summary>
        public MxfService() { }

        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// Use IDs such as s1, s2, s3, and so forth.
        /// </summary>
        [XmlAttribute("id")]
        public string Id
        {
            get
            {
                return ("s" + index.ToString());
            }
            set { }
        }

        /// <summary>
        /// An ID that uniquely identifies the service.
        /// Should be of the form "!Service!name", where name is the value of the name attribute.
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get
            {
                return ("!Service!EPG123_" + StationID);
            }
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