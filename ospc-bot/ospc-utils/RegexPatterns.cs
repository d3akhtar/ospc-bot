using System.Text.RegularExpressions;

namespace OSPC.Utils
{
    public static class RegexPatterns
    {
        public static Regex OsuBeatmapLinkRegex = new Regex(@"https://osu.ppy.sh/beatmapsets/([0-9]+)#osu/([0-9]+)");
        public static Regex StrictUsernameRegex = new Regex(@"^[\sa-zA-Z0-9\-_]{2,}$");
    }
}
