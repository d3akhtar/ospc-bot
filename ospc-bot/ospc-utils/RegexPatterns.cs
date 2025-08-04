using System.Text.RegularExpressions;

namespace OSPC.Utils
{
    public static class RegexPatterns
    {
        public static Regex OsuBeatmapLinkRegex = new Regex(@"https://osu.ppy.sh/beatmapsets/([0-9]+)#osu/([0-9]+)");
        public static Regex StrictUsernameRegex = new Regex(@"^[\sa-zA-Z0-9\-_]{2,}$");
        public static Regex UsernameRegex = new Regex(@"[a-zA-Z0-9\-_]{3,}|""[\sa-zA-Z0-9\-_]{3,}""");
        public static Regex FlagRegex = new Regex(@"-([e])");
        public static Regex InputFlagRegex = new Regex(@"-([atq]) (([a-zA-Z0-9\-_]+)|(""[\sa-zA-Z0-9\-_]+""))");
        public static Regex PlaycountComparisonRegex = new Regex(@"(playcount|pc|count|c)(<|<=|=|>|>=)([0-9]+)");
        public static string BeatmapFilterRegexTemplate = @"({abbreviation})(<|<=|=|>|>=)([0-9]+(\.[0-9]+)?)";
    }
}