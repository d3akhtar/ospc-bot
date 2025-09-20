using Microsoft.Extensions.Logging;

using MySql.Data.MySqlClient;

using OSPC.Domain.Model;
using OSPC.Infrastructure.Database.CommandFactory;
using OSPC.Infrastructure.Database.TransactionFactory;
using OSPC.Utils;
using OSPC.Utils.Cache;
using OSPC.Utils.Parsing;

namespace OSPC.Infrastructure.Database.Repository
{
    public class BeatmapRepository : IBeatmapRepository
    {
        private readonly ILogger<BeatmapRepository> _logger;
        private readonly IDatabase _db;
        private readonly ICommandFactory _commandFactory;
        private readonly ITransactionFactory _transactionFactory;

        public BeatmapRepository(ILogger<BeatmapRepository> logger, IDatabase db, ICommandFactory commandFactory, ITransactionFactory transactionFactory)
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
                var result = await _transactionFactory.CreateAddBeatmapPlaycountTransaction(conn, beatmapPlaycounts);
                if (!result.Successful)
                    return;
                else
                    await _db.CommitTransactionAsync(result.Value!);
            });
        }

        public async Task AddBeatmapsAsync(IEnumerable<Beatmap> beatmaps)
        {
            _logger.LogDebug("Adding {Count} new beatmaps", beatmaps.Count());

            await _db.ExecuteAsync(async conn =>
            {
                var result = await _transactionFactory.CreateAddBeatmapTransaction(conn, beatmaps);
                if (!result.Successful)
                    return;
                else
                    await _db.CommitTransactionAsync(result.Value!);
            });
        }

        public async Task AddBeatmapSetsAsync(List<BeatmapSet> beatmapSets)
        {
            _logger.LogDebug("Adding {Count} new beatmap sets", beatmapSets.Count);

            await _db.ExecuteAsync(async conn =>
            {
                var result = await _transactionFactory.CreateAddBeatmapSetTransaction(conn, beatmapSets);
                if (!result.Successful)
                    return;
                else
                    await _db.CommitTransactionAsync(result.Value!);
            });
        }

        public async Task<Result<List<BeatmapPlaycount>>> FilterBeatmapPlaycountsForUser
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
                var result = _commandFactory.CreateBeatmapPlaycountFilterCommand(conn, searchParams, userId, pageSize, pageNumber);
                if (!result.Successful)
                    return result.Error!;

                using var command = result.Value!;
                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                    res.Add(reader.ReadBeatmapPlaycountInclude());
                reader.Close();

                if (res.Count == 0)
                    return Errors.NotFound("No beatmaps match the given query");
                else
                    return res;
            });
        }

        public async Task<Result<Beatmap>> GetBeatmapById(int id)
        {
            _logger.LogDebug("Getting beatmap with id: {BeatmapId}", id);

            string key = CacheKey.ConvertTypeToKey<Beatmap>((id, "mapid"));
            return await _db.ExecuteCommandAsync<Beatmap>(key, async (conn) =>
            {
                var result = _commandFactory.CreateGetBeatmapByIdCommand(conn, id);
                if (!result.Successful)
                    return result.Error!;

                using var command = result.Value!;
                var reader = await command.ExecuteReaderAsync();
                if (reader.Read())
                {
                    var res = reader.ReadBeatmap();
                    reader.Close();
                    return res;
                }
                else
                    return Errors.NotFound("Beatmap not found");
            });
        }

        public async Task<Result<List<BeatmapPlaycount>>> GetBeatmapPlaycountsForUser(int userId, int pageSize, int pageNumber)
            => await FilterBeatmapPlaycountsForUser(SearchParams.Empty, userId, pageSize, pageNumber);

        public async Task<Result<BeatmapSet>> GetBeatmapSetById(int id)
        {
            _logger.LogDebug("Getting beatmapset with id: {BeatmapSetId}", id);

            string key = CacheKey.ConvertTypeToKey<BeatmapSet>((id, "setid"));
            return await _db.ExecuteCommandAsync<BeatmapSet>(key, async (conn) =>
            {
                var result = _commandFactory.CreateGetBeatmapSetByIdCommand(conn, id);
                if (!result.Successful)
                    return result.Error!;

                using var command = result.Value!;
                var reader = await command.ExecuteReaderAsync();
                if (reader.Read())
                {
                    var res = reader.ReadBeatmapSet();
                    reader.Close();
                    return res;
                }
                else
                    return Errors.NotFound("Beatmap set not found");
            });
        }

        public async Task<Result<int?>> GetPlaycountForBeatmap(int userId, int beatmapId)
        {
            _logger.LogDebug("Get playcount for userId: {UserId} on beatmapId: {BeatmapId}", userId, beatmapId);
            var bpcResult = await GetBeatmapPlaycountForUserOnMap(userId, beatmapId);
            return bpcResult.Successful switch
            {
                true => bpcResult.Value!.Count,
                false => bpcResult.Error!
            };
        }

        public async Task<Result<BeatmapPlaycount>> GetBeatmapPlaycountForUserOnMap(int userId, int beatmapId)
        {
            _logger.LogDebug("Get beatmap playcount info for userId: {UserId} on beatmapId: {BeatmapId}", userId, beatmapId);

            string key = CacheKey.ConvertTypeToKey<BeatmapPlaycount>((userId, "pc_userid"), (beatmapId, "pc_mapid"));
            return await _db.ExecuteCommandAsync<BeatmapPlaycount>(key, async (conn) =>
            {
                var result = _commandFactory.CreateGetBeatmapPlaycountForUserCommand(conn, userId, beatmapId);
                if (!result.Successful)
                    return result.Error!;

                using var command = result.Value!;
                var reader = await command.ExecuteReaderAsync();
                if (reader.Read())
                {
                    var res = reader.ReadBeatmapPlaycountInclude();
                    reader.Close();
                    return res;
                }
                else
                    return Errors.NotFound("Beatmap set not found");
            });
        }

        public async Task UpdateReferencedBeatmapIdForChannel(ulong channelId, int beatmapId)
        {
            _logger.LogDebug("Updating channelId: {ChannelId} last referenced beatmapId: {BeatmapId}", channelId, beatmapId);

            List<string> invalidatedKeys = [CacheKey.ConvertTypeToKey<int?>((channelId, "channelId"))];
            await _db.ExecuteInsertAsync(invalidatedKeys, async (conn) =>
            {
                var result = _commandFactory.CreateUpdateReferencedBeatmapIdForChannelCommand(conn, channelId, beatmapId);
                if (!result.Successful)
                {
                    _logger.LogError("Error occured during insert {@Error}", result.Error!);
                    return false;
                }

                using var command = result.Value!;
                return await command.ExecuteNonQueryAsync() > 0;
            });
        }

        public async Task<Result<int?>> GetReferencedBeatmapIdForChannel(ulong channelId)
        {
            _logger.LogDebug("Get referenced beatmapId for channelId: {ChannelId}", channelId);

            return await _db.ExecuteCommandAsync<int?>(
                CacheKey.ConvertTypeToKey<int?>((channelId, "channelId")),
                async (conn) =>
                {
                    var result = _commandFactory.CreateGetReferencedBeatmapIdForChannelCommand(conn, channelId);
                    if (!result.Successful)
                        return result.Error!;

                    using var command = result.Value!;
                    var reader = await command.ExecuteReaderAsync();
                    if (reader.Read())
                    {
                        var res = reader.GetInt32(0);
                        reader.Close();
                        return res;
                    }
                    else
                        return Errors.NotFound("Couldn't find last referenced beatmap in channel");
                });
        }

        public async Task<Result<int?>> GetTotalResultCountForSearch(SearchParams searchParams, int userId)
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
            return await _db.ExecuteCommandAsync<int?>(key, async (conn) =>
            {
                var result = _commandFactory.CreateBeatmapPlaycountFilterCommand(conn, searchParams, userId);
                if (!result.Successful)
                    return result.Error!;

                using var command = result.Value!;
                var reader = await command.ExecuteReaderAsync();
                if (reader.Read())
                {
                    var res = reader.GetInt32(0);
                    reader.Close();
                    return res;
                }
                else
                    return Errors.NotFound("Couldn't find any results for given query");
            });
        }

        public async Task<List<int>> GetRemainingBeatmapIds(HashSet<int> beatmapIds)
        {
            _logger.LogDebug("Get beatmapIds not in set: {BeatmapIdSet}", beatmapIds);

            return await _db.ExecuteAsync<List<int>>(async conn =>
            {
                if (beatmapIds.Count == 0)
                    return new();
                string param = "";
                int i = 0, length = beatmapIds.Count;
                foreach (int id in beatmapIds)
                {
                    param += $"{id}{(i < length - 1 ? "," : "")}";
                    i++;
                }
                string query = $"SELECT Id FROM Beatmaps WHERE Id IN ({param})";
                using var command = new MySqlCommand(query, conn);
                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                    beatmapIds.Remove(reader.GetInt32(0));
                reader.Close();
                return beatmapIds.ToList();
            });
        }
    }
}
