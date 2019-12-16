using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HDHomeRunTV;
using MxfXml;
using XmltvXml;

namespace hdhr2mxf
{
    public static class Common
    {
        public static MXF mxf = new MXF();
        public static HDHRAPI api = new HDHRAPI();

        public static HashSet<string> channelsDone = new HashSet<string>();
        public static bool noLogos = false;

        public static string ListContains(string[] categories, string match, bool exact = false)
        {
            if (categories == null) return null;
            else return ListContains(categories.ToList(), match, exact);
        }
        public static string ListContains(List<XmltvText> categories, string match, bool exact = false)
        {
            List<string> cats = new List<string>();
            foreach (XmltvText category in categories)
            {
                cats.Add(category.Text);
            }
            return ListContains(cats, match, exact);
        }
        public static string ListContains(List<string> categories, string match, bool exact = false)
        {
            if (categories != null)
            {
                foreach (string category in categories)
                {
                    if (exact && category.ToLower().Equals(match.ToLower()) ||
                        (!exact && category.ToLower().Contains(match.ToLower())))
                        return "true";
                }
            }
            return null;
        }

        #region ========== Keywords ==========
        private enum GROUPS { EDUCATIONAL, KIDS, MOVIES, NEWS, REALITY, SERIES, SPECIAL, SPORTS, PREMIERES, PAIDPROGRAMMING, UNKNOWN };
        private static string[] groups = { "Educational", "Kids", "Movies", "News", "Reality", "Series", "Special", "Sports", "Premieres", "Paid Programming" };

        public static bool buildKeywords()
        {
            for (int groupIdx = 0; groupIdx < groups.Length; ++groupIdx)
            {
                // build keywords from keywordgroups
                foreach (KeyValuePair<string, string> category in mxf.With[0].KeywordGroups[groupIdx].cats)
                {
                    mxf.With[0].Keywords.Add(new MxfKeyword()
                    {
                        Word = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(category.Key.ToLower()),
                        Id = category.Value
                    });
                }

                // create an overflow group to give us a possible 200 categories per group
                int overflow = groupIdx + groups.Length;
                mxf.With[0].KeywordGroups.Add(new MxfKeywordGroup() { index = groupIdx + 1, alpha = string.Empty });
                mxf.With[0].KeywordGroups[overflow].getKeywordId(groups[groupIdx]);
                mxf.With[0].KeywordGroups[overflow].index = (groupIdx + 1) * 1000;
                mxf.With[0].KeywordGroups[overflow].getKeywordId("All");

                // populate the overflow group starting with the 100th category
                for (int i = 99; i < mxf.With[0].KeywordGroups[groupIdx].sorted.Count; ++i)
                {
                    mxf.With[0].KeywordGroups[overflow].cats.Add(mxf.With[0].KeywordGroups[groupIdx].sorted.ElementAt(i).Key,
                                                                 mxf.With[0].KeywordGroups[groupIdx].sorted.ElementAt(i).Value);
                }
            }

            // now reverse order all keywordgroups in order to display properly in WMC
            List<MxfKeywordGroup> newGroups = new List<MxfKeywordGroup>();
            for (int i = 0; i < groups.Length; ++i)
            {
                newGroups.Add(mxf.With[0].KeywordGroups[groups.Length + i]);
                newGroups.Add(mxf.With[0].KeywordGroups[i]);
            }
            mxf.With[0].KeywordGroups = newGroups;

            return true;
        }

        public static void initializeKeywordGroups()
        {
            for (int i = 0; i < groups.Length; ++i)
            {
                mxf.With[0].KeywordGroups.Add(new MxfKeywordGroup() { index = i + 1, alpha = "m1" });
                mxf.With[0].KeywordGroups[i].getKeywordId(groups[i]);
                mxf.With[0].KeywordGroups[i].index = (i + 1) * 1000;
                mxf.With[0].KeywordGroups[i].getKeywordId("All");
            }
        }

        public static void determineProgramKeywords(ref MxfProgram mxfProgram, List<XmltvText> programCategories)
        {
            List<string> cats = new List<string>();
            foreach (XmltvText category in programCategories)
            {
                cats.Add(category.Text);
            }
            determineProgramKeywords(ref mxfProgram, cats.ToArray());
        }
        public static void determineProgramKeywords(ref MxfProgram mxfProgram, string[] programCategories)
        {
            // determine primary group of program
            GROUPS group = GROUPS.UNKNOWN;
            if (!string.IsNullOrEmpty(mxfProgram.IsMovie)) group = GROUPS.MOVIES;
            else if (!string.IsNullOrEmpty(mxfProgram.IsPaidProgramming)) group = GROUPS.PAIDPROGRAMMING;
            else if (!string.IsNullOrEmpty(mxfProgram.IsSports)) group = GROUPS.SPORTS;
            else if (!string.IsNullOrEmpty(mxfProgram.IsKids)) group = GROUPS.KIDS;
            else if (!string.IsNullOrEmpty(mxfProgram.IsEducational)) group = GROUPS.EDUCATIONAL;
            else if (!string.IsNullOrEmpty(mxfProgram.IsNews)) group = GROUPS.NEWS;
            else if (!string.IsNullOrEmpty(mxfProgram.IsSpecial)) group = GROUPS.SPECIAL;
            else if (!string.IsNullOrEmpty(mxfProgram.IsReality)) group = GROUPS.REALITY;
            else if (!string.IsNullOrEmpty(mxfProgram.IsSeries)) group = GROUPS.SERIES;

            // build the keywords/categories
            if (group != GROUPS.UNKNOWN)
            {
                mxfProgram.Keywords = string.Format("k{0}", (int)group + 1);

                // add premiere categories as necessary
                if (!string.IsNullOrEmpty(mxfProgram.IsSeasonPremiere) || !string.IsNullOrEmpty(mxfProgram.IsSeriesPremiere))
                {
                    mxfProgram.Keywords += string.Format(",k{0}", (int)GROUPS.PREMIERES + 1);
                    if (!string.IsNullOrEmpty(mxfProgram.IsSeasonPremiere)) mxfProgram.Keywords += "," + mxf.With[0].KeywordGroups[(int)GROUPS.PREMIERES].getKeywordId("Season Premiere");
                    if (!string.IsNullOrEmpty(mxfProgram.IsSeriesPremiere)) mxfProgram.Keywords += "," + mxf.With[0].KeywordGroups[(int)GROUPS.PREMIERES].getKeywordId("Series Premiere");
                }
                else if (!string.IsNullOrEmpty(mxfProgram.IsPremiere) && string.IsNullOrEmpty(mxfProgram.IsGeneric))
                {
                    if (group == GROUPS.MOVIES)
                    {
                        mxfProgram.Keywords += "," + mxf.With[0].KeywordGroups[(int)group].getKeywordId("Premiere");
                    }
                    else if (!string.IsNullOrEmpty(mxfProgram.IsMiniseries))
                    {
                        mxfProgram.Keywords += string.Format(",k{0}", (int)GROUPS.PREMIERES + 1);
                        mxfProgram.Keywords += "," + mxf.With[0].KeywordGroups[(int)GROUPS.PREMIERES].getKeywordId("Miniseries Premiere");
                    }
                    else if (!string.IsNullOrEmpty(mxfProgram.IsSeries))
                    {
                        mxfProgram.Keywords += string.Format(",k{0}", (int)GROUPS.PREMIERES + 1);
                        mxfProgram.Keywords += "," + mxf.With[0].KeywordGroups[(int)GROUPS.PREMIERES].getKeywordId("Series/Season Premiere");
                    }
                }

                // now add the real categories
                if (programCategories != null)
                {
                    foreach (string category in programCategories)
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
                                string key = mxf.With[0].KeywordGroups[(int)group].getKeywordId(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(category.ToLowerInvariant()));
                                List<string> keys = mxfProgram.Keywords.Split(',').ToList();
                                if (!keys.Contains(key))
                                {
                                    mxfProgram.Keywords += "," + key;
                                }
                                break;
                        }
                    }
                }
                if (mxfProgram.Keywords.Length < 5)
                {
                    string key = mxf.With[0].KeywordGroups[(int)group].getKeywordId("Uncategorized");
                    mxfProgram.Keywords += "," + key;
                }
            }
        }
        #endregion
    }
}