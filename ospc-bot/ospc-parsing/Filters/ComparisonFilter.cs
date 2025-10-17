using OSPC.Parsing.Converters;
using OSPC.Parsing.Regex.NamedGroupMatchValues;

namespace OSPC.Parsing.Filters
{
    public class ComparisonFilter
    {
        public static ComparisonFilter Create(NumberComparisonValue comparisonValue)
            => comparisonValue.IsBetweenComparison ? ParseBetweenFilter(comparisonValue) : ParseSingleFilter(comparisonValue);

        private static SingleFilter ParseSingleFilter(NumberComparisonValue comparisonValue)
            => new SingleFilter(float.Parse(comparisonValue.Value), ComparisonConverter.Convert(comparisonValue.Comparison));

        private static BetweenFilter ParseBetweenFilter(NumberComparisonValue comparisonValue)
            => new BetweenFilter(
                new SingleFilter(float.Parse(comparisonValue.Value), ComparisonConverter.Convert(comparisonValue.Comparison)),
                new SingleFilter(float.Parse(comparisonValue.SecondaryValue!), ComparisonConverter.Convert(comparisonValue.SecondaryComparison!))
            );
    }    
}
