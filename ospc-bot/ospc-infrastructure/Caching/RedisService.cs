using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OSPC.Domain.Model;
using OSPC.Domain.Options;
using OSPC.Utils.Cache;
using StackExchange.Redis;

namespace OSPC.Infrastructure.Caching
{
    public class RedisService : IRedisService
    {
        private readonly ILogger<RedisService> _logger;
        private readonly IOptions<CacheOptions> _cacheOptions;
        private readonly IConnectionMultiplexer _connection;
        private readonly IDatabase _cache;

        public RedisService(ILogger<RedisService> logger, IOptions<CacheOptions> cacheOptions, IConnectionMultiplexer connection)
        {            
            _logger = logger;
            _cacheOptions = cacheOptions;
            _connection = connection;
            _cache = _connection!.GetDatabase();
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            _logger.LogDebug("Getting access token from cache");
            var accessToken = await _cache.StringGetAsync(CacheKey.ACCESS_TOKEN);
            if (!accessToken.HasValue) _logger.LogWarning("Access token not found in cache");
            return accessToken;
        }

        public async Task<bool> SetAccessTokenAsync(string accessToken, int expiryTime)
        {
            _logger.LogDebug("Setting access token in cache");

            return await _cache.StringSetAsync(
                CacheKey.ACCESS_TOKEN, 
                accessToken, 
                TimeSpan.FromSeconds(expiryTime));
        }

        public async Task<T?> GetQuery<T>(string key)
        {
            _logger.LogDebug("Finding item in cache with key: {Key}", key);
            
            string? res = await _cache.StringGetAsync(key);

            if (res == null) {
                _logger.LogDebug("Item with key: {Key} not found in cache", key);
                return default;
            } else return JsonSerializer.Deserialize<T>(res);
        }

        public async Task InvalidateKeys(List<string> keys)
        {
            foreach (string key in keys){
                _logger.LogDebug("Invalidating cache key: {Key}", key);
                await _cache.KeyDeleteAsync(key);
            }
        }

        public async Task<bool> SaveQuery<T>(string key, T res)
        {
            _logger.LogDebug("Saving query with key: {Key}, value: {Value}", key, res);
            return await _cache.StringSetAsync(
                key, 
                JsonSerializer.Serialize(res), 
                TimeSpan.FromSeconds(CacheExpiryTimes.GetExpiryTimeForType<T>())
            );
        }

        public async Task<UserRankStatistic?> GetUserRankStatisticAsync(int userId)
        {
            _logger.LogDebug("Getting user rank statistic for user with id: {UserId}", userId);
            var res = await _cache.StringGetAsync($"user_stat_{userId}");
            if (res.IsNull) {
                _logger.LogDebug("User rank statistic not found for user with id: {UserId}", userId);
                return null;
            } else return JsonSerializer.Deserialize<UserRankStatistic>(res!);
        }

        public async Task<bool> SaveUserRankStatisticAsync(int userId, UserRankStatistic stats)
        {
            _logger.LogDebug("Saving user rank statistics for user with id: {UserId}, statistics: {@Statistics}", userId, stats);
            
            return await _cache.StringSetAsync(
                    $"user_stat_{userId}", 
                    JsonSerializer.Serialize(stats), 
                    TimeSpan.FromSeconds(CacheExpiryTimes.USER_RANK_STAT));
        }
    }
}
