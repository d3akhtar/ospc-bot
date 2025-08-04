using System.Text.RegularExpressions;

namespace OSPC.Utils
{
    public class ComparisonFilter 
    {
        public static SingleFilter GetSingleFilterFromMatch(GroupCollection group)
        {
            Comparison comparison = ComparisonConverter.Convert(group[2].Value);
            float value = float.Parse(group[3].Value);
            return new SingleFilter { Value = value, Comparison = comparison };
        }
        
        public static BetweenFilter GetBetweenFilterFromMatch(GroupCollection a, GroupCollection b)
        {
            SingleFilter filterA = GetSingleFilterFromMatch(a);
            SingleFilter filterB = GetSingleFilterFromMatch(b);
            
            if (filterA.Comparison == Comparison.Less && filterB.Comparison == Comparison.Greater) return new() { Min = filterB, Max = filterA };
            else if (filterA.Comparison == Comparison.LessOrEqual && filterB.Comparison == Comparison.Greater) return new() { Min = filterB, Max = filterA };
            else if (filterA.Comparison == Comparison.LessOrEqual && filterB.Comparison == Comparison.GreaterOrEqual) return new() { Min = filterB, Max = filterA };
            else if (filterB.Comparison == Comparison.Less && filterA.Comparison == Comparison.Greater) return new() { Min = filterA, Max = filterB };
            else if (filterB.Comparison == Comparison.LessOrEqual && filterA.Comparison == Comparison.Greater) return new() { Min = filterA, Max = filterB };
            else if (filterB.Comparison == Comparison.LessOrEqual && filterA.Comparison == Comparison.GreaterOrEqual) return new() { Min = filterA, Max = filterB };
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
    }

    public class BetweenFilter : ComparisonFilter
    {
        public required SingleFilter Min { get; set; }
        public required SingleFilter Max { get; set; }
        public override string ToString()
		    => $"{Min}{Max}";
    }

    public class SingleFilter : ComparisonFilter
    {
        public float Value { get; set; }
        public Comparison Comparison { get; set; }
        public override string ToString()
		    => $"{Comparison}{Value}";
    }
}
