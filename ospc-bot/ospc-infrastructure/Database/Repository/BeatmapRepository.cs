using MySql.Data.MySqlClient;
using OSPC.Domain.Model;
using OSPC.Utils;

namespace OSPC.Infrastructure.Database.Repository
{
    public class BeatmapRepository : IBeatmapRepository
    {
        private readonly DbContext _db;

        public BeatmapRepository(DbContext db)
        {
            _db = db;
        }

        public async Task AddBeatmapPlaycountsAsync(List<BeatmapPlaycount> beatmapPlaycounts)
        {
            await _db.ExecuteAsync(async conn => 
            {               
                using var transaction = await TransactionFactory.CreateAddBeatmapPlaycountTransaction(conn, beatmapPlaycounts);
                try {                  
                    await transaction.CommitAsync();
                } catch (Exception e) {
                    Console.WriteLine($"Error while adding beatmap playcount: {e.Message}");
                    await transaction.RollbackAsync();
                }
            });
        }

        public async Task AddBeatmapsAsync(IEnumerable<Beatmap> beatmaps)
        {
            await _db.ExecuteAsync(async conn =>
            {
                using var transaction = await TransactionFactory.CreateAddBeatmapTransaction(conn, beatmaps);
                try {
                    await transaction.CommitAsync();
                } catch (Exception e) {
                    Console.WriteLine($"Error while adding beatmaps: {e.Message}");
                    await transaction.RollbackAsync();
                }
            });
        }

        public async Task AddBeatmapSetsAsync(List<BeatmapSet> beatmapSets)
        {
            await _db.ExecuteAsync(async conn =>
            {            
                using var transaction = await TransactionFactory.CreateAddBeatmapSetTransaction(conn, beatmapSets);
                try {
                    await transaction.CommitAsync();
                } catch (Exception e) {
                    Console.WriteLine($"Error while adding beatmap sets: {e.Message}");
                    await transaction.RollbackAsync();
                }
            });
        }

        public async Task<List<BeatmapPlaycount>> FilterBeatmapPlaycountsForUser
            (SearchParams searchParams, int userId, int pageSize, int pageNumber)
        {
            string key = CacheKey.ConvertTypeToKey<List<BeatmapPlaycount>>(
                    (userId, "osuid"),
                    (searchParams.Query, "searchQuery"),
                    (searchParams.Artist, "searchArtist"),
                    (searchParams.Title, "searchTitle"),
                    (pageSize, "pageSize"),
                    (pageNumber, "pageNumber"),
                    (searchParams.Playcount, "pc"),
                    (searchParams.Exact, "exact"),
                    (searchParams.BeatmapFilter, "beatmapFilter")
            );

            return await _db.ExecuteCommandAsync<List<BeatmapPlaycount>>(key, async (conn) =>
            {
                List<BeatmapPlaycount> res = new();
                using var command = searchParams.CreateBeatmapPlaycountFilterCommand(conn, userId, pageSize, pageNumber);
                var reader = await command.ExecuteReaderAsync();
                while (reader.Read()) res.Add(reader.ReadBeatmapPlaycountInclude());
                reader.Close();
                return res;
            });
        }

        public async Task<Beatmap?> GetBeatmapById(int id)
        {
            string key = CacheKey.ConvertTypeToKey<Beatmap>((id, "mapid"));
            return await _db.ExecuteCommandAsync<Beatmap?>(key, async (conn) =>
            {
                Beatmap? res = null;
                using var command = CommandFactory.CreateGetBeatmapByIdCommand(conn, id);
                var reader = await command.ExecuteReaderAsync();
                if (reader.Read()) res = reader.ReadBeatmap();
                reader.Close();
                return res;
            });
        }

        public async Task<List<BeatmapPlaycount>> GetBeatmapPlaycountsForUser(int userId, int pageSize, int pageNumber)
            => await FilterBeatmapPlaycountsForUser(SearchParams.Empty, userId, pageSize, pageNumber);

        public async Task<BeatmapSet?> GetBeatmapSetById(int id)
        {
            string key = CacheKey.ConvertTypeToKey<BeatmapSet>((id, "setid"));
            return await _db.ExecuteCommandAsync<BeatmapSet?>(key, async (conn) =>
            {
                BeatmapSet? res = null;
                using var command = CommandFactory.CreateGetBeatmapSetByIdCommand(conn, id);
                var reader = await command.ExecuteReaderAsync();
                if (reader.Read()) res = reader.ReadBeatmapSet();
                reader.Close();
                return res;
            });
        }

        public async Task<int?> GetPlaycountForBeatmap(int userId, int beatmapId)
        {
            string key = CacheKey.ConvertTypeToKey<int>((userId, "pc_userid"), (beatmapId, "pc_mapid"));
            return await _db.ExecuteCommandAsync<int?>(key, async (conn) =>
            {
                int? res = null;
                using var command = CommandFactory.CreateGetPlaycountForBeatmapCommand(conn, userId, beatmapId);
                var reader = await command.ExecuteReaderAsync();
                if (reader.Read()) res = reader.GetInt32(0);
                reader.Close();
                return res;
            });
        }

        public async Task<BeatmapPlaycount?> GetBeatmapPlaycountForUserOnMap(int userId, int beatmapId)
        {
            string key = CacheKey.ConvertTypeToKey<BeatmapPlaycount>((userId, "pc_userid"), (beatmapId, "pc_mapid"));
            return await _db.ExecuteCommandAsync<BeatmapPlaycount?>(key, async (conn) =>
            {
                BeatmapPlaycount? res = null;
                using var command = CommandFactory.CreateGetBeatmapPlaycountForUserCommand(conn, userId, beatmapId);
                var reader = await command.ExecuteReaderAsync();
                if (reader.Read()) res = reader.ReadBeatmapPlaycountInclude();
                reader.Close();
                return res;
            });
        }

        public async Task UpdateReferencedBeatmapIdForChannel(ulong channelId, int beatmapId)
        {
            List<string> invalidatedKeys = [CacheKey.ConvertTypeToKey<int>((channelId, "channelId"))];
            await _db.ExecuteInsertAsync(invalidatedKeys, async conn
                    => await CommandFactory
                                .CreateUpdateReferencedBeatmapIdForChannelCommand(conn, channelId, beatmapId)
                                .ExecuteNonQueryAsync() > 0);
        }

        public async Task<int?> GetReferencedBeatmapIdForChannel(ulong channelId)
        {
            return await _db.ExecuteCommandAsync<int?>(
                CacheKey.ConvertTypeToKey<int>((channelId, "channelId")), 
                async (conn) => {
                    int res = -1;
                    using var command = CommandFactory.CreateGetReferencedBeatmapIdForChannelCommand(conn, channelId);
                    var reader = await command.ExecuteReaderAsync();
                    if (reader.Read()) res = reader.GetInt32(0);
                    reader.Close();
                    return res;
                });
        }

        public async Task<int> GetTotalResultCountForSearch(SearchParams searchParams, int userId)
        {
            string key = CacheKey.ConvertTypeToKey<List<BeatmapPlaycount>>(
                    (userId, "rescount_osuid"),
                    (searchParams.Query, "searchQuery"),
                    (searchParams.Artist, "searchArtist"),
                    (searchParams.Title, "searchTitle"),
                    (searchParams.Playcount, "pc"),
                    (searchParams.Exact, "exact"),
                    (searchParams.BeatmapFilter, "beatmapFilter")
            );
            return (int)await _db.ExecuteCommandAsync<int?>(key, async (conn) =>
            {
                int? res = null;
                using var command = searchParams.CreateBeatmapPlaycountFilterCommand(conn, userId);
                var reader = await command.ExecuteReaderAsync();
                if (reader.Read()) res = reader.GetInt32(0);
                reader.Close();
                return res;
            });
        }
        
        public async Task<List<int>> GetRemainingBeatmapIds(HashSet<int> beatmapIds)
        {
            return await _db.ExecuteAsync<List<int>>(async conn =>
            {
                if (beatmapIds.Count == 0) return new();
                string param = "";
                int i = 0, length = beatmapIds.Count;
                foreach (int id in beatmapIds){
                    param += $"{id}{(i < length - 1 ? ",":"")}";
                    i++;
                }
                string query = $"SELECT Id FROM Beatmaps WHERE Id IN ({param})";
                using var command = new MySqlCommand(query, conn);
                var reader = await command.ExecuteReaderAsync();
                while (reader.Read()) beatmapIds.Remove(reader.GetInt32(0));
                reader.Close();
                return beatmapIds.ToList();
            });
        }
    }
}
