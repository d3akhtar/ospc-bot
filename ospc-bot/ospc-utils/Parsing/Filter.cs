using OSPC.Utils.Parsing.RegularExpressions.NamedGroupMatchValues;

namespace OSPC.Utils
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

    public class BetweenFilter : ComparisonFilter
    {
        public SingleFilter Min { get; set; }
        public SingleFilter Max { get; set; }

        public BetweenFilter()
        {
            Min = new SingleFilter(0, Comparison.None);
            Max = new SingleFilter(0, Comparison.None);
        }

        public BetweenFilter(SingleFilter a, SingleFilter b)
        {
            if (a is null || b is null)
                new BetweenFilter();

            if (a!.Value <= b!.Value)
            {
                Min = a;
                Max = b;
            }
            else
            {
                Min = b;
                Max = a;
            }
        }

        public override string ToString()
            => $"{Min}{Max}";
    }

    public class SingleFilter : ComparisonFilter
    {
        public float Value { get; set; }
        public Comparison Comparison { get; set; }

        public SingleFilter(float value, Comparison comp)
        {
            Value = value;
            Comparison = comp;
        }

        public override string ToString()
            => $"{Comparison}{Value}";
    }
}