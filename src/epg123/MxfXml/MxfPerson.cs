using System.Collections.Generic;
using System.Xml.Serialization;

namespace epg123.MxfXml
{
    public partial class Mxf
    {
        private readonly Dictionary<string, MxfPerson> _people = new Dictionary<string, MxfPerson>();
        public MxfPerson GetPersonId(string name)
        {
            if (_people.TryGetValue(name, out var person)) return person;
            With.People.Add(person = new MxfPerson
            {
                Index = With.People.Count + 1,
                Name = name
            });
            _people.Add(name, person);
            return person;
        }
    }

    public class MxfPerson
    {
        public override string ToString() { return Id; }

        [XmlIgnore] public int Index;

        /// <summary>
        /// An ID that is unique to the document and defines this element.
        /// Use IDs such as p1, p2, and p3. Person elements are referenced by the ActorRole, GuestActorRole, HostRole, WriterRole, ProducerRole, and DirectorRole elements.
        /// </summary>
        [XmlAttribute("id")]
        public string Id
        {
            get => $"p{Index}";
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