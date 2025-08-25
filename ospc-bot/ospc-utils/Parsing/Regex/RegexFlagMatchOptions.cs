namespace OSPC.Utils.Parsing.RegularExpressions
{
	[Flags]
	public enum RegexFlagMatchOptions
	{
		NoInput,
		Word,
		Specials,
		Numbers,
		Multi
	}

	public static class RegexFlagMatchOptionsExtensions
	{
		private static RegexFlagMatchOptions[] flags = (RegexFlagMatchOptions[])Enum.GetValues(typeof(RegexFlagMatchOptions));
		public static string GetRegexString(this RegexFlagMatchOptions options)
		{
			throw new NotImplementedException();
		}
	}
}
