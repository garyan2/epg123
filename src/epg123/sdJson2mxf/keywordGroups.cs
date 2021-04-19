using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using epg123.MxfXml;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private enum keygroups { EDUCATIONAL, KIDS, MOVIES, NEWS, REALITY, SERIES, SPECIAL, SPORTS, PREMIERES, PAIDPROGRAMMING, UNKNOWN };
        private static readonly string[] Groups = { "Educational", "Kids", "Movies", "News", "Reality", "Series", "Special", "Sports", "Premieres", "Paid Programming" };

        private static void InitializeKeywordGroups()
        {
            for (var i = 0; i < Groups.Length; ++i)
            {
                SdMxf.With.KeywordGroups.Add(new MxfKeywordGroup() { Index = i + 1, Alpha = "m1" });
                SdMxf.With.KeywordGroups[i].GetKeywordId(Groups[i]);
                SdMxf.With.KeywordGroups[i].Index = (i + 1) * 1000;
                SdMxf.With.KeywordGroups[i].GetKeywordId("All");
            }
        }

        private static bool BuildKeywords()
        {
            for (var groupIdx = 0; groupIdx < Groups.Length; ++groupIdx)
            {
                // build keywords from keywordgroups
                foreach (var category in SdMxf.With.KeywordGroups[groupIdx].Cats)
                {
                    SdMxf.With.Keywords.Add(new MxfKeyword()
                    {
                        Word = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(category.Key.ToLower()),
                        Id = category.Value
                    });
                }

                // create an overflow group to give us a possible 200 categories per group
                var overflow = groupIdx + Groups.Length;
                SdMxf.With.KeywordGroups.Add(new MxfKeywordGroup() { Index = groupIdx + 1, Alpha = string.Empty });
                SdMxf.With.KeywordGroups[overflow].GetKeywordId(Groups[groupIdx]);
                SdMxf.With.KeywordGroups[overflow].Index = (groupIdx + 1) * 1000;
                SdMxf.With.KeywordGroups[overflow].GetKeywordId("All");

                // populate the overflow group starting with the 100th category
                for (var i = 99; i < SdMxf.With.KeywordGroups[groupIdx].Sorted.Count; ++i)
                {
                    SdMxf.With.KeywordGroups[overflow].Cats.Add(SdMxf.With.KeywordGroups[groupIdx].Sorted.ElementAt(i).Key,
                                                                   SdMxf.With.KeywordGroups[groupIdx].Sorted.ElementAt(i).Value);
                }
            }

            // now reverse order all keywordgroups in order to display properly in WMC
            var newGroups = new List<MxfKeywordGroup>();
            for (var i = 0; i < Groups.Length; ++i)
            {
                newGroups.Add(SdMxf.With.KeywordGroups[Groups.Length + i]);
                newGroups.Add(SdMxf.With.KeywordGroups[i]);
            }
            SdMxf.With.KeywordGroups = newGroups;
            Logger.WriteVerbose("Completed compiling keywords and keyword groups.");
            return true;
        }
    }
}