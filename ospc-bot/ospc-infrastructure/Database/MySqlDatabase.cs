using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using OSPC.Domain.Options;
using OSPC.Infrastructure.Caching;

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
            
            await using var connection =  new MySqlConnection(_databaseOptions.Value.ConnectionString);
            await connection.OpenAsync();
            return await action(connection);
        }

        public async Task ExecuteAsync(Func<MySqlConnection, Task> action)
        {
            _logger.LogDebug("Executing MySQL action");

            await using var connection =  new MySqlConnection(_databaseOptions.Value.ConnectionString);
            await connection.OpenAsync();
            await action(connection);
        }

        public async Task<T> ExecuteCommandAsync<T>(string key, Func<MySqlConnection, Task<T>> action)
        {
            _logger.LogDebug("Executing MySQL command for type: {@TypeInfo}", typeof(T));

            T? res = await _redis.GetQuery<T>(key);
            if (res is not null) return res;
            else {
                _logger.LogDebug("Cache key: {Key} not found, retreiving from database", key);
                await using var connection =  new MySqlConnection(_databaseOptions.Value.ConnectionString);
                await connection.OpenAsync();
                res = await action(connection);
                if (res != null) await _redis.SaveQuery(key, res);
                else _logger.LogDebug("Couldn't find anything from database");
                return res;
            }
        }

        public async Task<bool> ExecuteInsertAsync(List<string> invalidatedKeys, Func<MySqlConnection, Task<bool>> action)
        {
            _logger.LogDebug("Executing MySQL insert command");
            
            await _redis.InvalidateKeys(invalidatedKeys);
            await using var connection =  new MySqlConnection(_databaseOptions.Value.ConnectionString);
            await connection.OpenAsync();
            return await action(connection);
        }
    }
}
