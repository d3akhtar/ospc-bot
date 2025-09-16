using OSPC.Utils;
using OSPC.Utils.Parsing.RegularExpressions.NamedGroupMatchValues;

namespace OSPC.Tests.Unit
{
    public class ComparisonUtilsTests
    {
        [Theory]
        [InlineData("<", "10", "Beatmaps", "cs", "AND Beatmaps.cs<10")]
        [InlineData("<=", "8.1", "Beatmaps", "ar", "AND Beatmaps.ar<=8.1")]
        [InlineData("=", "2.2", "Beatmaps", "sr", "AND Beatmaps.sr=2.2")]
        [InlineData(">=", "3", "Beatmaps", "length", "AND Beatmaps.length>=3")]
        [InlineData(">", "1", "Beatmaps", "drain", "AND Beatmaps.drain>1")]
        public void CreateComparisonClauseFromSingleNumberComparisonTest(string comparisonStr, string value, string table, string attribute, string expected)
        {
            var compValue = new NumberComparisonValue(string.Empty, comparisonStr, value);
            AssertComparisonClauseCreation(table, attribute, expected, compValue);
        }

        [Theory]
        [InlineData(">", "8", "<", "10", "Beatmaps", "cs", "AND Beatmaps.cs>8 AND Beatmaps.cs<10")]
        [InlineData(">=", "8", "<=", "10", "Beatmaps", "cs", "AND Beatmaps.cs>=8 AND Beatmaps.cs<=10")]
        [InlineData(">", "8", "<=", "10", "Beatmaps", "cs", "AND Beatmaps.cs>8 AND Beatmaps.cs<=10")]
        [InlineData(">=", "8", "<", "10", "Beatmaps", "cs", "AND Beatmaps.cs>=8 AND Beatmaps.cs<10")]
        public void CreateComparisonClauseFromBetweenNumberComparisonTest(string comparisonStr, string value, string comparisonStr1, string value1, string table, string attribute, string expected)
        {
            var compValue = new NumberComparisonValue(string.Empty, comparisonStr, value, comparisonStr1, value1);
            AssertComparisonClauseCreation(table, attribute, expected, compValue);
        }

        [Fact]
        public void ComparisonFilterCreateFromSingleNumberComparisonTest()
        {
            NumberComparisonValue comp = new("Unnamed", "<=", "5");
            var filter = ComparisonFilter.Create(comp);

            Assert.IsType<SingleFilter>(filter);
            var singleFilter = filter as SingleFilter;
            Assert.NotNull(singleFilter);

            Assert.Equivalent(Comparison.LessOrEqual, singleFilter.Comparison);
            Assert.Equivalent(5, singleFilter.Value);
        }

        [Fact]
        public void ComparisonFilterCreateFromBetweenNumberComparisonTest()
        {
            NumberComparisonValue comp = new("Unnamed", ">=", "5", "<=", "10");
            var filter = ComparisonFilter.Create(comp);

            Assert.IsType<BetweenFilter>(filter);
            var betweenFilter = filter as BetweenFilter;
            Assert.NotNull(betweenFilter);

            Assert.Equivalent(Comparison.GreaterOrEqual, betweenFilter.Min.Comparison);
            Assert.Equivalent(5, betweenFilter.Min.Value);

            Assert.Equivalent(Comparison.LessOrEqual, betweenFilter.Max.Comparison);
            Assert.Equivalent(10, betweenFilter.Max.Value);
        }

        private static void AssertComparisonClauseCreation(string table, string attribute, string expected, NumberComparisonValue compValue)
        {
            var comparisonFilter = ComparisonFilter.Create(compValue);
            var clause = ComparisonConverter.CreateComparisonClause(comparisonFilter, table, attribute);

            Assert.Equivalent(expected, clause.Trim());
        }
    }
}