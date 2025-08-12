using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using OSPC.Domain.Model;
using OSPC.Infrastructure.Database.CommandFactory;
using OSPC.Infrastructure.Database.TransactionFactory;
using OSPC.Utils;

namespace OSPC.Infrastructure.Database.Repository
{
    public class BeatmapRepository : IBeatmapRepository
    {
        private readonly ILogger<BeatmapRepository> _logger;
        private readonly DbContext _db;
        private readonly ICommandFactory _commandFactory;
        private readonly ITransactionFactory _transactionFactory;

        public BeatmapRepository(ILogger<BeatmapRepository> logger, DbContext db, ICommandFactory commandFactory, ITransactionFactory transactionFactory)
        {
            _logger = logger;
            _db = db;
            _commandFactory = commandFactory;
            _transactionFactory = transactionFactory;
        }

        public async Task AddBeatmapPlaycountsAsync(List<BeatmapPlaycount> beatmapPlaycounts)
        {
            _logger.LogDebug("Adding {Count} new beatmap playcounts", beatmapPlaycounts.Count);
            
            await _db.ExecuteAsync(async conn => 
            {               
                using var transaction = await _transactionFactory.CreateAddBeatmapPlaycountTransaction(conn, beatmapPlaycounts);
                try {                  
                    await transaction.CommitAsync();
                } catch (Exception e) {
                    _logger.LogError(e, "Error while adding beatmap playcounts, rolling back transaction");
                    await transaction.RollbackAsync();
                }
            });
        }

        public async Task AddBeatmapsAsync(IEnumerable<Beatmap> beatmaps)
        {
            _logger.LogDebug("Adding {Count} new beatmaps", beatmaps.Count());
            
            await _db.ExecuteAsync(async conn =>
            {
                using var transaction = await _transactionFactory.CreateAddBeatmapTransaction(conn, beatmaps);
                try {
                    await transaction.CommitAsync();
                } catch (Exception e) {
                    _logger.LogError(e, "Error while adding beatmaps, rolling back transaction");
                    await transaction.RollbackAsync();
                }
            });
        }

        public async Task AddBeatmapSetsAsync(List<BeatmapSet> beatmapSets)
        {
            _logger.LogDebug("Adding {Count} new beatmap sets", beatmapSets.Count);
            
            await _db.ExecuteAsync(async conn =>
            {            
                using var transaction = await _transactionFactory.CreateAddBeatmapSetTransaction(conn, beatmapSets);
                try {
                    await transaction.CommitAsync();
                } catch (Exception e) {
                    _logger.LogError(e, "Error while adding beatmap sets, rolling back transaction");
                    await transaction.RollbackAsync();
                }
            });
        }

        public async Task<List<BeatmapPlaycount>> FilterBeatmapPlaycountsForUser
            (SearchParams searchParams, int userId, int pageSize, int pageNumber)
        {
            _logger.LogDebug("Filtering beatmap playcounts using searchParams: {@SearchParams}", searchParams);
        
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
                using var command = _commandFactory.CreateBeatmapPlaycountFilterCommand(conn, searchParams, userId, pageSize, pageNumber);
                var reader = await command.ExecuteReaderAsync();
                while (reader.Read()) res.Add(reader.ReadBeatmapPlaycountInclude());
                reader.Close();
                return res;
            });
        }

        public async Task<Beatmap?> GetBeatmapById(int id)
        {
            _logger.LogDebug("Getting beatmap with id: {BeatmapId}", id);
            
            string key = CacheKey.ConvertTypeToKey<Beatmap>((id, "mapid"));
            return await _db.ExecuteCommandAsync<Beatmap?>(key, async (conn) =>
            {
                Beatmap? res = null;
                using var command = _commandFactory.CreateGetBeatmapByIdCommand(conn, id);
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
            _logger.LogDebug("Getting beatmapset with id: {BeatmapSetId}", id);
            
            string key = CacheKey.ConvertTypeToKey<BeatmapSet>((id, "setid"));
            return await _db.ExecuteCommandAsync<BeatmapSet?>(key, async (conn) =>
            {
                BeatmapSet? res = null;
                using var command = _commandFactory.CreateGetBeatmapSetByIdCommand(conn, id);
                var reader = await command.ExecuteReaderAsync();
                if (reader.Read()) res = reader.ReadBeatmapSet();
                reader.Close();
                return res;
            });
        }

        public async Task<int?> GetPlaycountForBeatmap(int userId, int beatmapId)
        {
            _logger.LogDebug("Get playcount for userId: {UserId} on beatmapId: {BeatmapId}", userId, beatmapId);
            
            string key = CacheKey.ConvertTypeToKey<int>((userId, "pc_userid"), (beatmapId, "pc_mapid"));
            return await _db.ExecuteCommandAsync<int?>(key, async (conn) =>
            {
                int? res = null;
                using var command = _commandFactory.CreateGetPlaycountForBeatmapCommand(conn, userId, beatmapId);
                var reader = await command.ExecuteReaderAsync();
                if (reader.Read()) res = reader.GetInt32(0);
                reader.Close();
                return res;
            });
        }

        public async Task<BeatmapPlaycount?> GetBeatmapPlaycountForUserOnMap(int userId, int beatmapId)
        {
            _logger.LogDebug("Get beatmap playcount info for userId: {UserId} on beatmapId: {BeatmapId}", userId, beatmapId);
            
            string key = CacheKey.ConvertTypeToKey<BeatmapPlaycount>((userId, "pc_userid"), (beatmapId, "pc_mapid"));
            return await _db.ExecuteCommandAsync<BeatmapPlaycount?>(key, async (conn) =>
            {
                BeatmapPlaycount? res = null;
                using var command = _commandFactory.CreateGetBeatmapPlaycountForUserCommand(conn, userId, beatmapId);
                var reader = await command.ExecuteReaderAsync();
                if (reader.Read()) res = reader.ReadBeatmapPlaycountInclude();
                reader.Close();
                return res;
            });
        }

        public async Task UpdateReferencedBeatmapIdForChannel(ulong channelId, int beatmapId)
        {
            _logger.LogDebug("Updating channelId: {ChannelId} last referenced beatmapId: {BeatmapId}", channelId, beatmapId);

            List<string> invalidatedKeys = [CacheKey.ConvertTypeToKey<int>((channelId, "channelId"))];
            await _db.ExecuteInsertAsync(invalidatedKeys, async conn
                    => await _commandFactory
                                .CreateUpdateReferencedBeatmapIdForChannelCommand(conn, channelId, beatmapId)
                                .ExecuteNonQueryAsync() > 0);
        }

        public async Task<int?> GetReferencedBeatmapIdForChannel(ulong channelId)
        {
            _logger.LogDebug("Get referenced beatmapId for channelId: {ChannelId}", channelId);
            
            return await _db.ExecuteCommandAsync<int?>(
                CacheKey.ConvertTypeToKey<int>((channelId, "channelId")), 
                async (conn) => {
                    int res = -1;
                    using var command = _commandFactory.CreateGetReferencedBeatmapIdForChannelCommand(conn, channelId);
                    var reader = await command.ExecuteReaderAsync();
                    if (reader.Read()) res = reader.GetInt32(0);
                    reader.Close();
                    return res;
                });
        }

        public async Task<int> GetTotalResultCountForSearch(SearchParams searchParams, int userId)
        {
            _logger.LogDebug("Get userId: {UserId} total amount of results for searchParams: {@SearchParams}", userId, searchParams);
            
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
                using var command = _commandFactory.CreateBeatmapPlaycountFilterCommand(conn, searchParams, userId);
                var reader = await command.ExecuteReaderAsync();
                if (reader.Read()) res = reader.GetInt32(0);
                reader.Close();
                return res;
            });
        }
        
        public async Task<List<int>> GetRemainingBeatmapIds(HashSet<int> beatmapIds)
        {
            _logger.LogDebug("Get beatmapIds not in set: {BeatmapIdSet}", beatmapIds);
            
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
