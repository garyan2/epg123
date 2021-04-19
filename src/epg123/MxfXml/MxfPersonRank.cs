using System.Xml.Serialization;

namespace epg123.MxfXml
{
    public class MxfPersonRank
    {
        [XmlIgnore] public MxfPerson mxfPerson;

        /// <summary>
        /// A reference to the id value of the Person element.
        /// </summary>
        [XmlAttribute("person")]
        public string Person
        {
            get => mxfPerson?.ToString();
            set { }
        }

        /// <summary>
        /// Used to sort the names that are displayed.
        /// Lower numbers are displayed first.
        /// </summary>
        [XmlAttribute("rank")]
        public int Rank { get; set; }

        /// <summary>
        /// The role an actor plays
        /// </summary>
        [XmlAttribute("character")]
        public string Character { get; set; }
    }
}