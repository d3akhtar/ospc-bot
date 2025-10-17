namespace OSPC.Utils.Parsing.Regex
{
    public static class RegexFlagMatchOptionsExtensions
    {
        private static readonly RegexFlagMatchOptions[] appendableFlags = [
            RegexFlagMatchOptions.Word,
            RegexFlagMatchOptions.Specials,
            RegexFlagMatchOptions.Numbers,
        ];

        public static string GetRegexString(this RegexFlagMatchOptions options, string name)
        {
            if (options == RegexFlagMatchOptions.NoInput)
                return string.Empty;
            else if (options.HasFlag(RegexFlagMatchOptions.StrictlyNumbers))
                return options.HasFlag(RegexFlagMatchOptions.WholeNumbers) ?
                    @$"(?<{name}>{options.GetSignRegex()}[0-9]+)" :
                    @$"(?<{name}>{options.GetSignRegex()}[0-9]+(\.[0-9]+)?)";

            string regex = string.Empty;

            foreach (var flag in appendableFlags)
            {
                if (options.HasFlag(flag))
                    regex += flag.GetRegexForFlag();
            }

            if (options.HasFlag(RegexFlagMatchOptions.Multi))
                return $@"(?<{name}>([{regex}]+)|(?<{name}>""[\s{regex}]+""))";
            else
                return $"(?<{name}>[{regex}]+)";
        }

        public static bool ValidComparisonOptions(this RegexFlagMatchOptions flag)
            => (flag.HasFlag(RegexFlagMatchOptions.StrictlyNumbers) ||
                flag.HasFlag(RegexFlagMatchOptions.WholeNumbers) ||
                flag.HasFlag(RegexFlagMatchOptions.Positive)) &&
                !flag.HasFlag(RegexFlagMatchOptions.Multi);

        private static string GetRegexForFlag(this RegexFlagMatchOptions flag)
            => flag switch
            {
                RegexFlagMatchOptions.NoInput => "",
                RegexFlagMatchOptions.Word => @"a-zA-Z",
                RegexFlagMatchOptions.Specials => @"!@#$%^&*_\-",
                RegexFlagMatchOptions.Numbers => @"0-9",
                _ => ""
            };

        private static string GetSignRegex(this RegexFlagMatchOptions flag)
            => flag.HasFlag(RegexFlagMatchOptions.Positive) ?
                    "" : "[\\-]?";
    }
}