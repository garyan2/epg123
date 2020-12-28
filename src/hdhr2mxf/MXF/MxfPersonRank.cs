using System.Xml.Serialization;

namespace hdhr2mxf.MXF
{
    public class MxfPersonRank
    {
        /// <summary>
        /// A reference to the id value of the Person element.
        /// </summary>
        [XmlAttribute("person")]
        public string Person { get; set; }

        /// <summary>
        /// Used to sort the names that are displayed.
        /// Lower numbers are displayed first.
        /// </summary>
        [XmlAttribute("rank")]
        public string Rank { get; set; }

        /// <summary>
        /// The role an actor plays
        /// hasExtendedCastAndCrew must be "true" to display this information
        /// </summary>
        [XmlAttribute("character")]
        public string Character { get; set; }
    }
}
