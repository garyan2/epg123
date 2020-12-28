using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using hdhr2mxf.HDHR;
using hdhr2mxf.MXF;
using hdhr2mxf.XMLTV;

namespace hdhr2mxf
{
    public static class Common
    {
        public static mxf Mxf = new mxf();
        public static hdhrapi Api = new hdhrapi();

        public static HashSet<string> ChannelsDone = new HashSet<string>();
        public static bool NoLogos = false;

        public static string ListContains(string[] categories, string match, bool exact = false)
        {
            return categories == null ? null : ListContains(categories.ToList(), match, exact);
        }
        public static string ListContains(List<XmltvText> categories, string match, bool exact = false)
        {
            var cats = categories.Select(category => category.Text).ToList();
            return ListContains(cats, match, exact);
        }
        public static string ListContains(List<string> categories, string match, bool exact = false)
        {
            if (categories == null) return null;
            return categories.Any(category => exact && category.ToLower().Equals(match.ToLower()) ||
                                              (!exact && category.ToLower().Contains(match.ToLower()))) ? "true" : null;
        }

        #region ========== Keywords ==========
        private enum keygroups { EDUCATIONAL, KIDS, MOVIES, NEWS, REALITY, SERIES, SPECIAL, SPORTS, PREMIERES, PAIDPROGRAMMING, UNKNOWN };
        private static readonly string[] Groups = { "Educational", "Kids", "Movies", "News", "Reality", "Series", "Special", "Sports", "Premieres", "Paid Programming" };

        public static bool BuildKeywords()
        {
            for (var groupIdx = 0; groupIdx < Groups.Length; ++groupIdx)
            {
                // build keywords from keywordgroups
                foreach (var category in Mxf.With[0].KeywordGroups[groupIdx].Cats)
                {
                    Mxf.With[0].Keywords.Add(new MxfKeyword()
                    {
                        Word = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(category.Key.ToLower()),
                        Id = category.Value
                    });
                }

                // create an overflow group to give us a possible 200 categories per group
                var overflow = groupIdx + Groups.Length;
                Mxf.With[0].KeywordGroups.Add(new MxfKeywordGroup() { Index = groupIdx + 1, Alpha = string.Empty });
                Mxf.With[0].KeywordGroups[overflow].GetKeywordId(Groups[groupIdx]);
                Mxf.With[0].KeywordGroups[overflow].Index = (groupIdx + 1) * 1000;
                Mxf.With[0].KeywordGroups[overflow].GetKeywordId("All");

                // populate the overflow group starting with the 100th category
                for (var i = 99; i < Mxf.With[0].KeywordGroups[groupIdx].Sorted.Count; ++i)
                {
                    Mxf.With[0].KeywordGroups[overflow].Cats.Add(Mxf.With[0].KeywordGroups[groupIdx].Sorted.ElementAt(i).Key,
                                                                 Mxf.With[0].KeywordGroups[groupIdx].Sorted.ElementAt(i).Value);
                }
            }

            // now reverse order all keywordgroups in order to display properly in WMC
            var newGroups = new List<MxfKeywordGroup>();
            for (var i = 0; i < Groups.Length; ++i)
            {
                newGroups.Add(Mxf.With[0].KeywordGroups[Groups.Length + i]);
                newGroups.Add(Mxf.With[0].KeywordGroups[i]);
            }
            Mxf.With[0].KeywordGroups = newGroups;

            return true;
        }

        public static void InitializeKeywordGroups()
        {
            for (var i = 0; i < Groups.Length; ++i)
            {
                Mxf.With[0].KeywordGroups.Add(new MxfKeywordGroup() { Index = i + 1, Alpha = "m1" });
                Mxf.With[0].KeywordGroups[i].GetKeywordId(Groups[i]);
                Mxf.With[0].KeywordGroups[i].Index = (i + 1) * 1000;
                Mxf.With[0].KeywordGroups[i].GetKeywordId("All");
            }
        }

        public static void DetermineProgramKeywords(ref MxfProgram mxfProgram, List<XmltvText> programCategories)
        {
            DetermineProgramKeywords(ref mxfProgram, programCategories.Select(category => category.Text).ToArray());
        }
        public static void DetermineProgramKeywords(ref MxfProgram mxfProgram, string[] programCategories)
        {
            // determine primary group of program
            var group = keygroups.UNKNOWN;
            if (!string.IsNullOrEmpty(mxfProgram.IsMovie)) group = keygroups.MOVIES;
            else if (!string.IsNullOrEmpty(mxfProgram.IsPaidProgramming)) group = keygroups.PAIDPROGRAMMING;
            else if (!string.IsNullOrEmpty(mxfProgram.IsSports)) group = keygroups.SPORTS;
            else if (!string.IsNullOrEmpty(mxfProgram.IsKids)) group = keygroups.KIDS;
            else if (!string.IsNullOrEmpty(mxfProgram.IsEducational)) group = keygroups.EDUCATIONAL;
            else if (!string.IsNullOrEmpty(mxfProgram.IsNews)) group = keygroups.NEWS;
            else if (!string.IsNullOrEmpty(mxfProgram.IsSpecial)) group = keygroups.SPECIAL;
            else if (!string.IsNullOrEmpty(mxfProgram.IsReality)) group = keygroups.REALITY;
            else if (!string.IsNullOrEmpty(mxfProgram.IsSeries)) group = keygroups.SERIES;

            // build the keywords/categories
            if (group == keygroups.UNKNOWN) return;
            mxfProgram.Keywords = $"k{(int) group + 1}";

            // add premiere categories as necessary
            if (!string.IsNullOrEmpty(mxfProgram.IsSeasonPremiere) || !string.IsNullOrEmpty(mxfProgram.IsSeriesPremiere))
            {
                mxfProgram.Keywords += $",k{(int) keygroups.PREMIERES + 1}";
                if (!string.IsNullOrEmpty(mxfProgram.IsSeasonPremiere)) mxfProgram.Keywords += "," + Mxf.With[0].KeywordGroups[(int)keygroups.PREMIERES].GetKeywordId("Season Premiere");
                if (!string.IsNullOrEmpty(mxfProgram.IsSeriesPremiere)) mxfProgram.Keywords += "," + Mxf.With[0].KeywordGroups[(int)keygroups.PREMIERES].GetKeywordId("Series Premiere");
            }
            else if (!string.IsNullOrEmpty(mxfProgram.IsPremiere) && string.IsNullOrEmpty(mxfProgram.IsGeneric))
            {
                if (group == keygroups.MOVIES)
                {
                    mxfProgram.Keywords += "," + Mxf.With[0].KeywordGroups[(int)group].GetKeywordId("Premiere");
                }
                else if (!string.IsNullOrEmpty(mxfProgram.IsMiniseries))
                {
                    mxfProgram.Keywords += $",k{(int) keygroups.PREMIERES + 1}";
                    mxfProgram.Keywords += "," + Mxf.With[0].KeywordGroups[(int)keygroups.PREMIERES].GetKeywordId("Miniseries Premiere");
                }
                else if (!string.IsNullOrEmpty(mxfProgram.IsSeries))
                {
                    mxfProgram.Keywords += $",k{(int) keygroups.PREMIERES + 1}";
                    mxfProgram.Keywords += "," + Mxf.With[0].KeywordGroups[(int)keygroups.PREMIERES].GetKeywordId("Series/Season Premiere");
                }
            }

            // now add the real categories
            if (programCategories != null)
            {
                foreach (var category in programCategories)
                {
                    switch (category.ToLower())
                    {
                        case "feature film":
                        case "short film":
                        case "tv movie":
                        case "miniseries":
                        case "series":
                        case "special":
                        case "sports event":
                        case "sports non-event":
                        case "paid programming":
                        case "theatre event":
                        case "show":
                        case "episode":
                        case "sports":
                        case "movie":
                            break;
                        default:
                            var key = Mxf.With[0].KeywordGroups[(int)group].GetKeywordId(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(category.ToLowerInvariant()));
                            var keys = mxfProgram.Keywords.Split(',').ToList();
                            if (!keys.Contains(key))
                            {
                                mxfProgram.Keywords += "," + key;
                            }
                            break;
                    }
                }
            }

            if (mxfProgram.Keywords.Length >= 5) return;
            {
                var key = Mxf.With[0].KeywordGroups[(int)group].GetKeywordId("Uncategorized");
                mxfProgram.Keywords += "," + key;
            }
        }
        #endregion
    }
}