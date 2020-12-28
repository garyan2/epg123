using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace hdhr2mxf.MXF
{
    public class MxfKeywordGroup
    {
        [XmlIgnore]
        public int Index;

        [XmlIgnore]
        public string Alpha = string.Empty;

        [XmlIgnore]
        public Dictionary<string, string> Cats = new Dictionary<string, string>();

        [XmlIgnore]
        public SortedDictionary<string, string> Sorted
        {
            get
            {
                var ret = new SortedDictionary<string, string>();
                for (var i = 2; i < Cats.Count; ++i)
                {
                    ret.Add(Cats.ElementAt(i).Key, Cats.ElementAt(i).Value);
                }
                return ret;
            }
            set { }
        }

        public string GetKeywordId(string keyword)
        {
            if (Cats.TryGetValue(keyword, out var ret)) return ret;
            ret = $"k{Index++}";
            Cats.Add(keyword, ret);
            return ret;
        }

        /// <summary>
        /// The value of a Keyword id attribute, and defines the name of the KeywordGroup.
        /// Each KeywordGroup name is displayed as the top-level words in the Search By Category page.
        /// </summary>
        [XmlAttribute("groupName")]
        public string GroupName
        {
            get => Cats.ElementAt(0).Value;
            set { }
        }

        /// <summary>
        /// A unique ID that will remain consistent between multiple versions of this document.
        /// This uid should start with "!KeywordGroup!".
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get => ("!KeywordGroup!" + GroupName + Alpha);
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
                var ret = Cats.ElementAt(1).Value + ",";
                for (var i = 0; i < Math.Min(Sorted.Count, 99); ++i)
                {
                    ret += Sorted.ElementAt(i).Value + ",";
                }
                ret = ret.TrimEnd(',');

                return ret;
            }
            set { }
        }
    }
}
