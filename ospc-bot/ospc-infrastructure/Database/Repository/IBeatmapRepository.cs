using OSPC.Domain.Model;
using OSPC.Utils;
using OSPC.Utils.Parsing;

namespace OSPC.Infrastructure.Database.Repository
{
    public interface IBeatmapRepository
    {
        public Task AddBeatmapsAsync(IEnumerable<Beatmap> beatmaps);
        public Task AddBeatmapPlaycountsAsync(List<BeatmapPlaycount> beatmapPlaycounts);
        public Task AddBeatmapSetsAsync(List<BeatmapSet> beatmapSets);
        public Task<Result<List<BeatmapPlaycount>>> GetBeatmapPlaycountsForUser(int userId, int pageSize, int pageNumber);
        public Task<Result<BeatmapPlaycount>> GetBeatmapPlaycountForUserOnMap(int userId, int beatmapId);
        public Task<Result<List<BeatmapPlaycount>>> FilterBeatmapPlaycountsForUser
            (SearchParams searchParams, int userId, int pageSize, int pageNumber);
        public Task<Result<int>> GetTotalResultCountForSearch(SearchParams searchParams, int userId);
        public Task<Result<int>> GetPlaycountForBeatmap(int userId, int beatmapId);
        public Task<Result<Beatmap>> GetBeatmapById(int id);
        public Task<Result<BeatmapSet>> GetBeatmapSetById(int id);
        public Task<List<int>> GetRemainingBeatmapIds(HashSet<int> beatmapIds);
        public Task UpdateReferencedBeatmapIdForChannel(ulong channelId, int beatmapId);
        public Task<Result<int>> GetReferencedBeatmapIdForChannel(ulong channelId);
    }
}