using OSPC.Domain.Model;

namespace OSPC.Utils.Cache
{
    public static class CacheKey
    {
        public static string ACCESS_TOKEN = "access_token";
        public static string USERNAME_TO_ID_BASE = "username_";

        private static readonly Dictionary<Type, string> _typeToCacheKeyBase = new()
        {
            { typeof(DiscordPlayer), "disc_pl" },
            { typeof(User), "user" },
            { typeof(Beatmap), "b" },
            { typeof(BeatmapPlaycount), "bpc" },
            { typeof(List<BeatmapPlaycount>), "bpc_list"},
            { typeof(BeatmapSet), "bs" },
            { typeof(int), "num" }
        };

        public static string ConvertTypeToKey<T>(params (object? Val, string Name)[] queryArgs)
            => $"{_typeToCacheKeyBase[typeof(T)]}:{queryArgs.Aggregate("", (acc, curr) => acc += $"{curr.Name}-{curr.Val ?? "null"}_")}";

    }
}