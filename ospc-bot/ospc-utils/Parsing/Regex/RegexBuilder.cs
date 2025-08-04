using System.Text;
using System.Text.RegularExpressions;

namespace OSPC.Utils.RegularExpressions
{
	public class RegexEvaluator
	{
		private StringBuilder _regexStringBuilder = new(string.Empty);
		
		public RegexEvaluator AddFlag(string name, params string[] flagAliases) => AddFlag(name, RegexFlagMatchOptions.NoInput, flagAliases);

		public RegexEvaluator AddFlag(string name, RegexFlagMatchOptions matchOptions, params string[] flagAliases)
		{
			if (_regexStringBuilder.Length > 0) _regexStringBuilder.Append('|');

			string flags = string.Empty;
			if (flagAliases.Length == 1) flags = flagAliases[0];
			else {
				for (int i = 0; i < flags.Length; i++){
					flags += flagAliases[i];
					if (i < flags.Length - 1) flags += '|';
				}
			}
			
			_regexStringBuilder
				.Append('(')
				.Append($"?<{name}>")
				.Append(flags)
				.Append(matchOptions.GetRegexString())
				.Append(')');

			return this;
		}

		public Regex ToRegex() => new(_regexStringBuilder.ToString());
	}
}
