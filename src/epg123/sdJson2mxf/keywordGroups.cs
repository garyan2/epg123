using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using epg123.MxfXml;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private enum keygroups { EDUCATIONAL, KIDS, MOVIES, NEWS, REALITY, SERIES, SPECIAL, SPORTS, PREMIERES, PAIDPROGRAMMING, UNKNOWN };
        private static readonly string[] Groups = { "Educational", "Kids", "Movies", "News", "Reality", "Series", "Special", "Sports", "Premieres", "Paid Programming" };

        private static void InitializeKeywordGroups()
        {
            foreach (var group in Groups)
            {
                SdMxf.GetKeywordGroup(group, "m1");
            }
        }

        private static bool BuildKeywords()
        {
            foreach (var group in SdMxf.With.KeywordGroups.ToList())
            {
                // create initial keywords for keyword group
                SdMxf.With.Keywords.AddRange(new List<MxfKeyword>
                {
                    new MxfKeyword { Index = group.Index, Word = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Groups[group.Index - 1]) },
                    new MxfKeyword { Index = group.Index * 1000, Word = "All" }
                });

                // add the keywords
                foreach (var keyword in group.mxfKeywords)
                {
                    SdMxf.With.Keywords.Add(keyword);
                }

                // create and populate an overflow for this group
                SdMxf.With.KeywordGroups.Add(new MxfKeywordGroup { Index = group.Index });
                if (group.mxfKeywords.Count <= 99) continue;
                var overflow = SdMxf.With.KeywordGroups.Last();
                overflow.mxfKeywords = group.mxfKeywords.OrderBy(k => k.Word).Skip(99).Take(99).ToList();
            }

            // now sort all keyword groups in order to display properly in WMC
            SdMxf.With.KeywordGroups = SdMxf.With.KeywordGroups.OrderBy(g => g.Index).ThenBy(g => g.Alpha).ToList();
            Logger.WriteVerbose("Completed compiling keywords and keyword groups.");
            return true;
        }
    }
}