using OSPC.Parsing.ParsedObjects;
using OSPC.Utils.Helpers;

using ComparisonFilter = OSPC.Parsing.Filters.ComparisonFilter;

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