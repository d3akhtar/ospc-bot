using OSPC.Utils.Helpers;

using BeatmapFilter = OSPC.Utils.Parsing.BeatmapFilter;
using ComparisonFilter = OSPC.Utils.ComparisonFilter;
using SearchParams = OSPC.Utils.Parsing.SearchParams;

namespace OSPC.Tests.Unit
{
    public class ExpressionHelpersTests
    {
        public void GetMemberNameTest()
        {
            Assert.Equivalent("username", ExpressionHelpers.GetMemberName<SearchParams, string>(sp => sp.Username));
            Assert.Equivalent("exact", ExpressionHelpers.GetMemberName<SearchParams, bool>(sp => sp.Exact));
            Assert.Equivalent("query", ExpressionHelpers.GetMemberName<SearchParams, string>(sp => sp.Query));
            Assert.Equivalent("artist", ExpressionHelpers.GetMemberName<SearchParams, string>(sp => sp.Artist!));
            Assert.Equivalent("title", ExpressionHelpers.GetMemberName<SearchParams, string>(sp => sp.Title!));
            Assert.Equivalent("playcount", ExpressionHelpers.GetMemberName<SearchParams, ComparisonFilter>(sp => sp.Playcount!));
            Assert.Equivalent("beatmapFilter", ExpressionHelpers.GetMemberName<SearchParams, BeatmapFilter>(sp => sp.BeatmapFilter!));
        }
    }
}