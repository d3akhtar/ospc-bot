using OSPC.Domain.Model;

namespace OSPC.Utils.Cache
{
    public static class CacheExpiryTimes
    {
        // Times are in seconds
        public const int DEFAULT = 3600; // 1 hour
        public const int USERNAME_TO_ID = 86400; // 24 hours
        public const int USER_RANK_STAT = 7200; // 2 hours
        private static readonly Dictionary<Type, int> _resTypeExpiryTime = new()
        {
            { typeof(DiscordPlayer), DEFAULT*24*30 },
            { typeof(User), DEFAULT*24*30 },
            { typeof(Beatmap), DEFAULT*24*30 },
            { typeof(BeatmapSet), DEFAULT*24*30 },
            { typeof(BeatmapPlaycount), DEFAULT/4},
            { typeof(List<BeatmapPlaycount>), DEFAULT/4},
            { typeof(int), DEFAULT/4}
        };

        public static int GetExpiryTimeForType<T>() => _resTypeExpiryTime.GetValueOrDefault(typeof(T), DEFAULT);
    }
}