using System.Xml.Serialization;

namespace hdhr2mxf.MXF
{
    public class MxfPerson
    {
        [XmlIgnore]
        public int Index;

        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// Use IDs such as p1, p2, and p3. Person elements are referenced by the ActorRole, GuestActorRole, HostRole, WriterRole, ProducerRole, and DirectorRole elements.
        /// </summary>
        [XmlAttribute("id")]
        public string Id
        {
            get => ("p" + Index.ToString());
            set { }
        }

        /// <summary>
        /// The name of the person.
        /// The maximum length is 160 characters.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// A unique ID that will remain consistent between multiple versions of this document.
        /// This uid should take the form "!Person!name", where name is the value of the name attribute.
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get => ("!Person!" + Name);
            set { }
        }
    }
}
