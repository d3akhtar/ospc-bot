using OSPC.Parsing.Converters;

namespace OSPC.Parsing.Filters
{
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
}
