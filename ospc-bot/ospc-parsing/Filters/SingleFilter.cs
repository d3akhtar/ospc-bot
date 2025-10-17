using OSPC.Parsing.Converters;

namespace OSPC.Parsing.Filters
{
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
