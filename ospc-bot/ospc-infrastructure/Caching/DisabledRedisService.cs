using OSPC.Domain.Model;

namespace OSPC.Infrastructure.Caching
{
    public class DisabledRedisService : IRedisService
    {
        public Task<string?> GetAccessTokenAsync() => Task.FromResult((string?)null);
        public Task<T?> GetQuery<T>(string key) => Task.FromResult<T?>(default);
        public Task<UserRankStatistic?> GetUserRankStatisticAsync(int userId) => Task.FromResult((UserRankStatistic?)null);
        public Task InvalidateKeys(List<string> keys) => Task.CompletedTask;
        public Task<bool> SaveQuery<T>(string key, T res) => Task.FromResult(true);
        public Task<bool> SaveUserRankStatisticAsync(int userId, UserRankStatistic stat) => Task.FromResult(true);
        public Task<bool> SetAccessTokenAsync(string accessToken, int expiryTime) => Task.FromResult(true);
    }
}
