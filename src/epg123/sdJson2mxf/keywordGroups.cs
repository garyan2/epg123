using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using epg123.MxfXml;

namespace epg123
{
    public static partial class sdJson2mxf
    {
        private enum GROUPS { EDUCATIONAL, KIDS, MOVIES, NEWS, REALITY, SERIES, SPECIAL, SPORTS, PREMIERES, PAIDPROGRAMMING, UNKNOWN };
        private static string[] groups = { "Educational", "Kids", "Movies", "News", "Reality", "Series", "Special", "Sports", "Premieres", "Paid Programming" };

        private static void initializeKeywordGroups()
        {
            for (int i = 0; i < groups.Length; ++i)
            {
                sdMxf.With[0].KeywordGroups.Add(new MxfKeywordGroup() { index = i + 1, alpha = "m1" });
                sdMxf.With[0].KeywordGroups[i].getKeywordId(groups[i]);
                sdMxf.With[0].KeywordGroups[i].index = (i + 1) * 1000;
                sdMxf.With[0].KeywordGroups[i].getKeywordId("All");
            }
        }

        private static bool buildKeywords()
        {
            for (int groupIdx = 0; groupIdx < groups.Length; ++groupIdx)
            {
                // build keywords from keywordgroups
                foreach (KeyValuePair<string, string> category in sdMxf.With[0].KeywordGroups[groupIdx].cats)
                {
                    sdMxf.With[0].Keywords.Add(new MxfKeyword()
                    {
                        Word = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(category.Key.ToLower()),
                        Id = category.Value
                    });
                }

                // create an overflow group to give us a possible 200 categories per group
                int overflow = groupIdx + groups.Length;
                sdMxf.With[0].KeywordGroups.Add(new MxfKeywordGroup() { index = groupIdx + 1, alpha = string.Empty });
                sdMxf.With[0].KeywordGroups[overflow].getKeywordId(groups[groupIdx]);
                sdMxf.With[0].KeywordGroups[overflow].index = (groupIdx + 1) * 1000;
                sdMxf.With[0].KeywordGroups[overflow].getKeywordId("All");

                // populate the overflow group starting with the 100th category
                for (int i = 99; i < sdMxf.With[0].KeywordGroups[groupIdx].sorted.Count; ++i)
                {
                    sdMxf.With[0].KeywordGroups[overflow].cats.Add(sdMxf.With[0].KeywordGroups[groupIdx].sorted.ElementAt(i).Key,
                                                                   sdMxf.With[0].KeywordGroups[groupIdx].sorted.ElementAt(i).Value);
                }
            }

            // now reverse order all keywordgroups in order to display properly in WMC
            List<MxfKeywordGroup> newGroups = new List<MxfKeywordGroup>();
            for (int i = 0; i < groups.Length; ++i)
            {
                newGroups.Add(sdMxf.With[0].KeywordGroups[groups.Length + i]);
                newGroups.Add(sdMxf.With[0].KeywordGroups[i]);
            }
            sdMxf.With[0].KeywordGroups = newGroups;
            Logger.WriteVerbose("Completed compiling keywords and keyword groups.");
            return true;
        }
    }
}