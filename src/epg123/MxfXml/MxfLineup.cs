using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123.MxfXml
{
    public class MxfLineup
    {
        public override string ToString() { return Id; }

        [XmlIgnore] public int Index;
        [XmlIgnore] public string LineupId;
        [XmlIgnore] public string UidOverride;

        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// Use the value l1.
        /// </summary>
        [XmlAttribute("id")]
        public string Id
        {
            get => $"l{Index}";
            set { }
        }

        /// <summary>
        /// An ID that uniquely identifies the lineup.
        /// The uid value should be in the form "!Lineup!uniqueLineupName", where uniqueLineupName is a unique ID for this lineup across all Lineup elements.
        /// Lesson learned: uid value should start with !MCLineup! -> this is the way to present information in about guide.
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get => string.IsNullOrEmpty(UidOverride) ? $"!MCLineup!{LineupId}" : $"!MCLineup!{UidOverride}";
            set { }
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
            get => Index <= 1 ? "!MCLineup!MainLineup" : null;
            set { }
        }

        [XmlArrayItem("Channel")]
        public List<MxfChannel> channels { get; set; } = new List<MxfChannel>();
    }
}