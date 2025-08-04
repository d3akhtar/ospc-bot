using OSPC.Domain.Model;

namespace OSPC.Infrastructure.Caching
{
    public interface IRedisService
    {
        public Task<string?> GetAccessTokenAsync();
        public Task<bool> SetAccessTokenAsync(string accessToken, int expiryTime);
        public Task<bool> SaveQuery<T>(string key, T res);
        public Task<T?> GetQuery<T>(string key);
        public Task InvalidateKeys(List<string> keys);
        public Task<UserRankStatistic?> GetUserRankStatisticAsync(int userId);
        public Task<bool> SaveUserRankStatisticAsync(int userId, UserRankStatistic stat);
    }
}