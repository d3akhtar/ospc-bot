using OSPC.Domain.Model;

namespace OSPC.Infrastructure.Http
{
    public interface IOsuWebClient
    {
        public Task<List<BeatmapPlaycount>> GetBeatmapPlaycountsForUser(int userId, int limit = 100, int offset = 0);
        public Task<List<Beatmap>> GetBeatmapStats(IEnumerable<int> beatmapIds);
        public Task<User?> FindUserWithUsername(string username);
        public Task LoadUserPlayedMaps(int userId);
        public Task<UserRankStatistic?> GetUserRankStatistics(int userId);

        // public Task<bool> UserOnline(User user);
        // public Task<bool> UserAccountDeleted(User user);
    }
}