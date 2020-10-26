using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace epg123.MxfXml
{
    public class MxfKeywordGroup
    {
        /// <summary>
        /// A set, or group, of keywords. The keyword group has a primary keyword name that identifies the group and is used to define the hierarchy in the Search By Category functionality.
        /// A KeywordGroup might have a Keyword name of Movies, and group the following keywords: Comedy, Drama, Horror, and Sci-Fi.
        /// </summary>
        public MxfKeywordGroup() { }

        /// <summary>
        /// The value of a Keyword id attribute, and defines the name of the KeywordGroup.
        /// Each KeywordGroup name is displayed as the top-level words in the Search By Category page.
        /// </summary>
        [XmlAttribute("groupName")]
        public string GroupName { get; set; }

        /// <summary>
        /// A unique ID that will remain consistent between multiple versions of this document.
        /// This uid should start with "!KeywordGroup!".
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid { get; set; }

        /// <summary>
        /// A comma-delimited ordered list of keyword IDs. This defines the keywords in this group.
        /// Used in the Search By Category page to display the list of keywords in the KeywordGroup element. The first keyword in this list should always be the "All" keyword.Programs should not be tagged with this keyword because it is a special placeholder to provide the localized value of "All".
        /// </summary>
        [XmlAttribute("keywords")]
        public string Keywords { get; set; }
    }
}
