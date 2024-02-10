using System.Collections.Generic;
using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    public partial class MXF
    {
        private readonly Dictionary<string, MxfLineup> _lineups = new Dictionary<string, MxfLineup>();
        public MxfLineup FindOrCreateLineup(string lineupId, string lineupName)
        {
            if (_lineups.TryGetValue(lineupId, out var lineup)) return lineup;
            With.Lineups.Add(lineup = new MxfLineup(With.Lineups.Count + 1, lineupId, lineupName));
            _lineups.Add(lineupId, lineup);
            return lineup;
        }
    }

    public class MxfLineup
    {
        public string LineupId => _lineupId;

        private string _uid;
        private int _index;
        private readonly string _lineupId;

        public MxfLineup(int index, string lineupId, string lineupName)
        {
            _index = index;
            _lineupId = lineupId;
            Name = lineupName;
        }
        private MxfLineup() { }

        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// Use the value l1.
        /// </summary>
        [XmlAttribute("id")]
        public string Id
        {
            get => $"l{_index}";
            set { _index = int.Parse(value.Substring(1)); }
        }

        /// <summary>
        /// An ID that uniquely identifies the lineup.
        /// The uid value should be in the form "!Lineup!uniqueLineupName", where uniqueLineupName is a unique ID for this lineup across all Lineup elements.
        /// Lesson learned: uid value should start with !MCLineup! -> this is the way to present information in about guide.
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get => _uid ?? $"!MCLineup!{_lineupId}";
            set { _uid = value; }
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
            get => _index <= 1 ? "!MCLineup!MainLineup" : null;
            set { }
        }

        [XmlArrayItem("Channel")]
        public List<MxfChannel> channels { get; set; } = new List<MxfChannel>();
    }
}