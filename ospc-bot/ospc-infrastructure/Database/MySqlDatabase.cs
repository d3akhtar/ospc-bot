using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MySql.Data.MySqlClient;

using OSPC.Domain.Options;
using OSPC.Infrastructure.Caching;
using OSPC.Utils;

namespace OSPC.Infrastructure.Database
{
    public class MySqlDatabase : IDatabase
    {
        private readonly ILogger<MySqlDatabase> _logger;
        private readonly IOptions<DatabaseOptions> _databaseOptions;
        private readonly IRedisService _redis;

        public MySqlDatabase(ILogger<MySqlDatabase> logger, IOptions<DatabaseOptions> databaseOptions, IRedisService redis)
        {
            _logger = logger;
            _databaseOptions = databaseOptions;
            _redis = redis;
        }

        public async Task<T> ExecuteAsync<T>(Func<MySqlConnection, Task<T>> action)
        {
            _logger.LogDebug("Executing MySQL action for type: {@TypeInfo}", typeof(T));

            await using var connection = new MySqlConnection(_databaseOptions.Value.ConnectionString);
            await connection.OpenAsync();
            return await action(connection);
        }

        public async Task ExecuteAsync(Func<MySqlConnection, Task> action)
        {
            _logger.LogDebug("Executing MySQL action");

            await using var connection = new MySqlConnection(_databaseOptions.Value.ConnectionString);
            await connection.OpenAsync();
            await action(connection);
        }

        public async Task<Result<T>> ExecuteCommandAsync<T>(string key, Func<MySqlConnection, Task<Result<T>>> action)
        {
            _logger.LogDebug("Executing MySQL command for type: {@TypeInfo}", typeof(T));

            if (await _redis.GetQuery<T?>(key) is {} cachedResult)
                return cachedResult;
            else
            {
                _logger.LogDebug("Cache key: {Key} not found, retreiving from database", key);
                await using var connection = new MySqlConnection(_databaseOptions.Value.ConnectionString);
                await connection.OpenAsync();
                var result = await action(connection);
                if (result.Successful)
                    await _redis.SaveQuery(key, result.Value);
                else
                    _logger.LogDebug("Couldn't find anything from database");

                _logger.LogDebug("Returning result for MYSQL command: {@Result}", result);
                return result;
            }
        }

        public async Task CommitTransactionAsync(MySqlTransaction transaction)
        {
            try
            {
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while commiting, rolling back transaction");
                await transaction.RollbackAsync();
            }
        }

        public async Task<bool> ExecuteInsertAsync(List<string> invalidatedKeys, Func<MySqlConnection, Task<bool>> action)
        {
            _logger.LogDebug("Executing MySQL insert command");

            await _redis.InvalidateKeys(invalidatedKeys);
            await using var connection = new MySqlConnection(_databaseOptions.Value.ConnectionString);
            await connection.OpenAsync();
            return await action(connection);
        }
    }
}
