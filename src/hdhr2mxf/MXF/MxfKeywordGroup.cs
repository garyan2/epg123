using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace MxfXml
{
    public class MxfKeywordGroup
    {
        [XmlIgnore]
        public int index;

        [XmlIgnore]
        public string alpha = string.Empty;

        [XmlIgnore]
        public Dictionary<string, string> cats = new Dictionary<string, string>();

        [XmlIgnore]
        public SortedDictionary<string, string> sorted
        {
            get
            {
                SortedDictionary<string, string> ret = new SortedDictionary<string, string>();
                for (int i = 2; i < cats.Count; ++i)
                {
                    ret.Add(cats.ElementAt(i).Key, cats.ElementAt(i).Value);
                }
                return ret;
            }
            set { }
        }

        public string getKeywordId(string keyword)
        {
            string ret = string.Empty;
            if (!cats.TryGetValue(keyword, out ret))
            {
                ret = string.Format("k{0}", index++);
                cats.Add(keyword, ret);
            }
            return ret;
        }

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
        public string GroupName
        {
            get
            {
                return cats.ElementAt(0).Value;
            }
            set { }
        }

        /// <summary>
        /// A unique ID that will remain consistent between multiple versions of this document.
        /// This uid should start with "!KeywordGroup!".
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get
            {
                return ("!KeywordGroup!" + GroupName + alpha);
            }
            set { }
        }

        /// <summary>
        /// A comma-delimited ordered list of keyword IDs. This defines the keywords in this group.
        /// Used in the Search By Category page to display the list of keywords in the KeywordGroup element. The first keyword in this list should always be the "All" keyword.Programs should not be tagged with this keyword because it is a special placeholder to provide the localized value of "All".
        /// </summary>
        [XmlAttribute("keywords")]
        public string Keywords
        {
            get
            {
                string ret = cats.ElementAt(1).Value + ",";
                for (int i = 0; i < Math.Min(sorted.Count, 99); ++i)
                {
                    ret += sorted.ElementAt(i).Value + ",";
                }
                ret = ret.TrimEnd(',');

                return ret;
            }
            set { }
        }
    }
}
