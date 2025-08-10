using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using OSPC.Domain.Options;
using OSPC.Infrastructure.Caching;

namespace OSPC.Infrastructure.Database
{
    public class DbContext
    {
        private readonly IOptions<DatabaseOptions> _databaseOptions;
        private readonly IRedisService _redis;

        public DbContext(IOptions<DatabaseOptions> databaseOptions, IRedisService redis)
        {
            _databaseOptions = databaseOptions;
            _redis = redis;
        }

        public async Task<T> ExecuteAsync<T>(Func<MySqlConnection, Task<T>> action)
        {
            await using var connection =  new MySqlConnection(_databaseOptions.Value.ConnectionString);
            await connection.OpenAsync();
            return await action(connection);
        }

        public async Task ExecuteAsync(Func<MySqlConnection, Task> action)
        {
            await using var connection =  new MySqlConnection(_databaseOptions.Value.ConnectionString);
            await connection.OpenAsync();
            await action(connection);
        }

        public async Task<T> ExecuteQueryAsync<T>(string query, string key, Func<MySqlConnection, string, Task<T>> action)
        {
            Console.WriteLine($"\nExecuting query: {query}");
            var command = new MySqlCommand();
            T? res = await _redis.GetQuery<T>(key);
            if (res != null) {
                Console.WriteLine("Cache hit");
                return res;
            }
            else {
                Console.WriteLine("Cache miss");
                await using var connection =  new MySqlConnection(_databaseOptions.Value.ConnectionString);
                await connection.OpenAsync();
                res = await action(connection, query);
                if (res != null) await _redis.SaveQuery(key, res);
                return res;
            }
        }

        public async Task<T> ExecuteCommandAsync<T>(string key, Func<MySqlConnection, Task<T>> action)
        {
            Console.WriteLine($"\nExecuting command, key: {key}");
            T? res = await _redis.GetQuery<T>(key);
            if (res is not null) {
                Console.WriteLine("Cache hit");
                return res;
            }
            else {
                Console.WriteLine("Cache miss");
                await using var connection =  new MySqlConnection(_databaseOptions.Value.ConnectionString);
                await connection.OpenAsync();
                res = await action(connection);
                if (res != null) await _redis.SaveQuery(key, res);
                return res;
            }
        }

        public async Task<bool> ExecuteInsertAsync(List<string> invalidatedKeys, Func<MySqlConnection, Task<bool>> action)
        {
            await _redis.InvalidateKeys(invalidatedKeys);
            await using var connection =  new MySqlConnection(_databaseOptions.Value.ConnectionString);
            await connection.OpenAsync();
            return await action(connection);
        }
    }
}
