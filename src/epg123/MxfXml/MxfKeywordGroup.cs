using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace epg123.MxfXml
{
    public partial class Mxf
    {
        private readonly Dictionary<string, MxfKeywordGroup> _keywordGroups = new Dictionary<string, MxfKeywordGroup>();

        public MxfKeywordGroup GetKeywordGroup(string groupName, string alpha = null)
        {
            if (_keywordGroups.TryGetValue(groupName, out var group)) return group;
            With.KeywordGroups.Add(group = new MxfKeywordGroup
            {
                Index = _keywordGroups.Count + 1,
                Alpha = alpha
            });
            _keywordGroups.Add(groupName, group);
            return group;
        }
    }

    public class MxfKeywordGroup
    {
        private readonly Dictionary<string, MxfKeyword> _keywords = new Dictionary<string, MxfKeyword>();
        public MxfKeyword GetKeyword(string word)
        {
            if (_keywords.TryGetValue(word, out var keyword)) return keyword;
            mxfKeywords.Add(keyword = new MxfKeyword
            {
                Index = Index * 1000 + mxfKeywords.Count + 1,
                Word = word
            });
            _keywords.Add(word, keyword);
            return keyword;
        }

        [XmlIgnore] public int Index;
        [XmlIgnore] public string Alpha = string.Empty;
        [XmlIgnore] public List<MxfKeyword> mxfKeywords = new List<MxfKeyword>();

        /// <summary>
        /// The value of a Keyword id attribute, and defines the name of the KeywordGroup.
        /// Each KeywordGroup name is displayed as the top-level words in the Search By Category page.
        /// </summary>
        [XmlAttribute("groupName")]
        public string GroupName
        {
            get => $"k{Index}";
            set { }
        }

        /// <summary>
        /// A unique ID that will remain consistent between multiple versions of this document.
        /// This uid should start with "!KeywordGroup!".
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get => $"!KeywordGroup!{GroupName}{Alpha}";
            set { }
        }

        /// <summary>
        /// A comma-delimited ordered list of keyword IDs. This defines the keywords in this group.
        /// Used in the Search By Category page to display the list of keywords in the KeywordGroup element.
        /// The first keyword in this list should always be the "All" keyword.
        /// Programs should not be tagged with this keyword because it is a special placeholder to provide the localized value of "All".
        /// </summary>
        [XmlAttribute("keywords")]
        public string Keywords
        {
            get => $"k{Index * 1000},{string.Join(",", mxfKeywords.OrderBy(k => k.Word).Select(k => k.Id).Take(99).ToArray())}".TrimEnd(',');
            set { }
        }
    }
}