using System.Text.Json;
using Microsoft.Extensions.Options;
using OSPC.Domain.Model;
using OSPC.Domain.Options;
using OSPC.Utils;
using StackExchange.Redis;

namespace OSPC.Infrastructure.Caching
{
    public class RedisService : IRedisService
    {
        private readonly IOptions<CacheOptions> _cacheOptions;
        private readonly ConnectionMultiplexer _connection;
        private readonly IDatabase _cache;

        public RedisService(IOptions<CacheOptions> cacheOptions)
        {
            _cacheOptions = cacheOptions;
            _connection = ConnectionMultiplexer.Connect(cacheOptions.Value.RedisConnection);
            _cache = _connection.GetDatabase();
        }

        public async Task<string?> GetAccessTokenAsync()
            => await _cache.StringGetAsync(CacheKey.ACCESS_TOKEN);

        public async Task<bool> SetAccessTokenAsync(string accessToken, int expiryTime)
            => await _cache.StringSetAsync(
                CacheKey.ACCESS_TOKEN, 
                accessToken, 
                TimeSpan.FromSeconds(expiryTime));

        public async Task<T?> GetQuery<T>(string key)
        {
            string? res = await _cache.StringGetAsync(key);
            return res == null ? default:JsonSerializer.Deserialize<T>(res);
        }

        public async Task InvalidateKeys(List<string> keys)
        {
            foreach (string key in keys){
                Console.WriteLine($"Invalidating key: {key}");
                await _cache.KeyDeleteAsync(key);
            }
        }

        public async Task<bool> SaveQuery<T>(string key, T res)
            => await _cache.StringSetAsync(
                key, 
                JsonSerializer.Serialize(res), 
                TimeSpan.FromSeconds(CacheExpiryTimes.GetExpiryTimeForType<T>())
            );

        public async Task<UserRankStatistic?> GetUserRankStatisticAsync(int userId)
        {
            var res = await _cache.StringGetAsync($"user_stat_{userId}");
            if (res.IsNull) return null;
            else return JsonSerializer.Deserialize<UserRankStatistic>(res!);
        }

        public async Task<bool> SaveUserRankStatisticAsync(int userId, UserRankStatistic stat)
            => await _cache.StringSetAsync(
                    $"user_stat_{userId}", 
                    JsonSerializer.Serialize(stat), 
                    TimeSpan.FromSeconds(CacheExpiryTimes.USER_RANK_STAT));
    }
}
