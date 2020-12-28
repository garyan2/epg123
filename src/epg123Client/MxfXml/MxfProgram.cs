using System.Xml.Serialization;

namespace epg123Client.MxfXml
{
    public class MxfProgram
    {
        /// <summary>
        /// A unique ID that will remain consistent between multiple versions of this document.
        /// This uid should start with "!Program!".
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid { get; set; }

        /// <summary>
        /// The title of the program (for example, Lost). 
        /// The maximum length is 512 characters.
        /// </summary>
        [XmlAttribute("title")]
        public string Title { get; set; }

        /// <summary>
        /// The episode title of the program (for example, The others attack).
        /// The maximum length is 512 characters.
        /// </summary>
        [XmlAttribute("episodeTitle")]
        public string EpisodeTitle { get; set; }
    }
}