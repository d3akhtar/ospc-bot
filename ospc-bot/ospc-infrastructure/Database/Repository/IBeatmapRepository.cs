using OSPC.Domain.Model;
using OSPC.Utils;

namespace OSPC.Infrastructure.Database.Repository
{
    public interface IBeatmapRepository
    {
        public Task AddBeatmapsAsync(IEnumerable<Beatmap> beatmaps);
        public Task AddBeatmapPlaycountsAsync(List<BeatmapPlaycount> beatmapPlaycounts);
        public Task AddBeatmapSetsAsync(List<BeatmapSet> beatmapSets);
        public Task<List<BeatmapPlaycount>> GetBeatmapPlaycountsForUser(int userId, int pageSize, int pageNumber);
        public Task<BeatmapPlaycount?> GetBeatmapPlaycountForUserOnMap(int userId, int beatmapId);
        public Task<List<BeatmapPlaycount>> FilterBeatmapPlaycountsForUser
            (SearchParams searchParams, int userId, int pageSize, int pageNumber);
        public Task<int> GetTotalResultCountForSearch(SearchParams searchParams, int userId);
        public Task<int?> GetPlaycountForBeatmap(int userId, int beatmapId);
        public Task<Beatmap?> GetBeatmapById(int id);
        public Task<BeatmapSet?> GetBeatmapSetById(int id);
        public Task<List<int>> GetRemainingBeatmapIds(HashSet<int> beatmapIds);
        public Task UpdateReferencedBeatmapIdForChannel(ulong channelId, int beatmapId);
        public Task<int?> GetReferencedBeatmapIdForChannel(ulong channelId);
    }
}