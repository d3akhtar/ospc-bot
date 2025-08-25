using System.Text.RegularExpressions;
using OSPC.Utils.Parsing.RegularExpressions.NamedGroupMatchValues;

namespace OSPC.Utils
{
    public class ComparisonFilter 
    {
        public static SingleFilter GetSingleFilterFromMatch(GroupCollection group)
        {
            Comparison comparison = ComparisonConverter.Convert(group[2].Value);
            float value = float.Parse(group[3].Value);
            return new SingleFilter(value, comparison);
        }
        
        public static BetweenFilter GetBetweenFilterFromMatch(GroupCollection a, GroupCollection b)
        {
            SingleFilter filterA = GetSingleFilterFromMatch(a);
            SingleFilter filterB = GetSingleFilterFromMatch(b);
            
            if (filterA.Comparison == Comparison.Less && filterB.Comparison == Comparison.Greater) return new(filterB, filterA);
            else if (filterA.Comparison == Comparison.LessOrEqual && filterB.Comparison == Comparison.Greater) return new(filterB, filterA);
            else if (filterA.Comparison == Comparison.LessOrEqual && filterB.Comparison == Comparison.GreaterOrEqual) return new(filterB, filterA);
            else if (filterB.Comparison == Comparison.Less && filterA.Comparison == Comparison.Greater) return new(filterA, filterB);
            else if (filterB.Comparison == Comparison.LessOrEqual && filterA.Comparison == Comparison.Greater) return new(filterA, filterB);
            else if (filterB.Comparison == Comparison.LessOrEqual && filterA.Comparison == Comparison.GreaterOrEqual) return new(filterA, filterB);
            else throw new ArgumentException("Invalid between filter");
        }

        public static (bool success, ComparisonFilter? filter) GetFilterFromMatch(MatchCollection matches)
        {
            switch (matches.Count) {
                case 0: return (true, null);
                case 1: return (true, GetSingleFilterFromMatch(matches[0].Groups));
                case 2: {
                    var filter = GetBetweenFilterFromMatch(matches[0].Groups, matches[1].Groups);
                    return (filter != null, filter);
                }
                default: return (false, null);
            }
        }

        public static ComparisonFilter Create(NumberComparisonValue comparisonValue)
            => comparisonValue.IsBetweenComparison ? ParseBetweenFilter(comparisonValue):ParseSingleFilter(comparisonValue);

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

        public BetweenFilter(SingleFilter a, SingleFilter b)
        {
            if (a.Value <= b.Value) {
                Min = a;
                Max = b;
            } else {
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
