using System.Collections.Generic;
using System.Xml.Serialization;

namespace MxfXml
{
    public class MxfLineup
    {
        [XmlIgnore]
        public int index;

        [XmlIgnore]
        public string uid_;

        /// <summary>
        /// Defines a collection of channels.
        /// </summary>
        public MxfLineup() { }

        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// Use the value l1.
        /// </summary>
        [XmlAttribute("id")]
        public string Id
        {
            get
            {
                return ("l" + index.ToString());
            }
            set { }
        }

        /// <summary>
        /// An ID that uniquely identifies the lineup.
        /// The uid value should be in the form "!Lineup!uniqueLineupName", where uniqueLineupName is a unique ID for this lineup across all Lineup elements.
        /// Lesson learned: uid value should start with !MCLineup! and uid should be numberic only -> this is the way to present information in about guide.
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get
            {
                return ("!MCLineup!" + uid_);
            }
            set
            {
                uid_ = value;
            }
        }

        /// <summary>
        /// The name of the lineup.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// The primary provider.
        /// This value should always be set to "!MCLineup!MainLineup".
        /// </summary>
        [XmlAttribute("primaryProvider")]
        public string PrimaryProvider
        {
            get
            {
                if (index == 1)
                {
                    return "!MCLineup!MainLineup";
                }
                return null;
            }
            set { }
        }

        [XmlArrayItem("Channel")]
        public List<MxfChannel> channels { get; set; }
    }
}
