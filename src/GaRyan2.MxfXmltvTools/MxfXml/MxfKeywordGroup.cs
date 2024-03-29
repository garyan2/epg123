﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace GaRyan2.MxfXml
{
    public partial class MXF
    {
        public enum KeywordGroups { EDUCATIONAL, KIDS, MOVIES, MUSIC, NEWS, PAIDPROGRAMMING, PREMIERES, REALITY, SERIES, SPECIAL, SPORTS, UNKNOWN };
        [XmlIgnore] public string[] KeywordGroupsText = { "Educational", "Kids", "Movies", "Music", "News", "Paid Programming", "Premieres", "Reality", "Series", "Special", "Sports" };

        private readonly Dictionary<string, MxfKeywordGroup> _keywordGroups = new Dictionary<string, MxfKeywordGroup>();
        public MxfKeywordGroup FindOrCreateKeywordGroup(KeywordGroups groupEnum, bool overflow = false)
        {
            var groupKey = $"{KeywordGroupsText[(int)groupEnum]}-{(!overflow ? "pri" : "ovf")}";
            if (_keywordGroups.TryGetValue(groupKey, out var group)) return group;
            With.KeywordGroups.Add(group = new MxfKeywordGroup((int)groupEnum + 1, !overflow ? "pri" : "ovf"));
            if (!overflow)
            {
                With.Keywords.AddRange(new List<MxfKeyword>
                {
                    new MxfKeyword(group.Index, group.Index, CultureInfo.CurrentCulture.TextInfo.ToTitleCase(KeywordGroupsText[group.Index - 1])),
                    new MxfKeyword(group.Index, group.Index * 1000, "All")
                });
            }
            _keywordGroups.Add(groupKey, group);
            return group;
        }
    }

    public class MxfKeywordGroup
    {
        [XmlIgnore] public int Index => _index;

        private string _uid;
        private int _index;
        private string _keywords;
        private readonly string _alpha;
        [XmlIgnore] public List<MxfKeyword> mxfKeywords;

        private readonly Dictionary<string, MxfKeyword> _Keywords = new Dictionary<string, MxfKeyword>();
        public MxfKeyword FindOrCreateKeyword(string word)
        {
            word = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(word);
            if (_Keywords.TryGetValue(word, out var keyword)) return keyword;
            mxfKeywords.Add(keyword = new MxfKeyword(_index, _index * 1000 + mxfKeywords.Count + 1, word));
            _Keywords.Add(word, keyword);
            return keyword;
        }

        public MxfKeywordGroup(int index, string alpha)
        {
            _index = index;
            _alpha = alpha ?? "";
            mxfKeywords = new List<MxfKeyword>();
        }
        private MxfKeywordGroup() { }

        /// <summary>
        /// The value of a Keyword id attribute, and defines the name of the KeywordGroup.
        /// Each KeywordGroup name is displayed as the top-level words in the Search By Category page.
        /// </summary>
        [XmlAttribute("groupName")]
        public string GroupName
        {
            get => $"k{_index}";
            set { _index = int.Parse(value.Substring(1)); }
        }

        /// <summary>
        /// A unique ID that will remain consistent between multiple versions of this document.
        /// This uid should start with "!KeywordGroup!".
        /// </summary>
        [XmlAttribute("uid")]
        public string Uid
        {
            get => _uid ?? $"!KeywordGroup!{GroupName}-{_alpha}";
            set { _uid = value; }
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
            get => _keywords ?? $"k{_index * 1000},{string.Join(",", mxfKeywords.OrderBy(k => k.Word).Select(k => k.Id).Take(99).ToArray())}".TrimEnd(',');
            set { _keywords = value; }
        }
    }
}