// TODO: Remember to change the folder name to match the namespace xd

namespace OSPC.Utils.Parsing.RegularExpressions.NamedGroupMatchValues
{
	public abstract record NamedGroupMatchValue(string Name);

	
	public record NoMatchValue(string Name) : NamedGroupMatchValue(Name);
	public record StringValue : NamedGroupMatchValue
	{
		public string Value;
	
		public StringValue(string name, string value) : base(name)
		{
			Value = value.Trim(' ', '\"');
		}
	}

	public record NumberComparisonValue : NamedGroupMatchValue
	{
		public string Comparison;
		public string Value;
		public string? SecondaryComparison = null;
		public string? SecondaryValue = null;
	
		public bool IsBetweenComparison => !string.IsNullOrEmpty(SecondaryComparison) && !string.IsNullOrEmpty(SecondaryValue);
	
		public NumberComparisonValue(string name, string comparison, string value) : base(name)
		{
			Comparison = comparison;
			Value = value;
		}
	
		public NumberComparisonValue(string name, string comparison, string value, string secondaryComparison, string secondaryValue) : base(name)
		{
			Comparison = comparison;
			Value = value;
			SecondaryComparison = secondaryComparison;
			SecondaryValue = secondaryValue;
		}
	
		public Result ValidNumberComparisonValue()
		{
			switch (Comparison) {
				case "<":
				case "<=": {
					if (SecondaryComparison is {} c && c.Contains('<')) return Errors.Parsing($"Cannot have two comparisons containing \'<\'");
					break;
				}
				case ">":
				case ">=": {
					if (SecondaryComparison is {} c && c.Contains('>')) return Errors.Parsing($"Cannot have two comparisons containing \'>\'");
					break;
				}
				case "=": 
					if (SecondaryComparison is {}) return Errors.Parsing($"Cannot have another comparisons alongside an \'=\' comparison");
					break;
				default: return Errors.Parsing($"Invalid comparison: {Comparison}");
			}
		
			return Result.Success();
		}
	}
}
