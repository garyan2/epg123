using GaRyan2.MxfXml;
using GaRyan2.Utilities;
using System.Linq;

namespace epg123.sdJson2mxf
{
    internal static partial class sdJson2Mxf
    {
        private static bool BuildKeywords()
        {
            foreach (var group in mxf.With.KeywordGroups.ToList())
            {
                // sort the group keywords
                group.mxfKeywords = group.mxfKeywords.OrderBy(k => k.Word).ToList();

                // add the keywords
                mxf.With.Keywords.AddRange(group.mxfKeywords);

                // create an overflow for this group giving a max 198 keywords for each group
                var overflow = mxf.FindOrCreateKeywordGroup((MXF.KeywordGroups)group.Index - 1, true);
                if (group.mxfKeywords.Count <= 99) continue;
                overflow.mxfKeywords = group.mxfKeywords.Skip(99).Take(99).ToList();
            }
            Logger.WriteVerbose("Completed compiling keywords and keyword groups.");
            return true;
        }
    }
}