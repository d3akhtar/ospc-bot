using MySql.Data.MySqlClient;
using OSPC.Infrastructure.Caching;

namespace OSPC.Infrastructure.Database
{
    public class DbContext
    {
        private readonly IRedisService _redis;
        public DbContext(IRedisService redis)
        {
            _redis = redis;
        }
        private readonly string _connectionString = "Server=localhost;Database=ospc;User=root;Password=;Pooling=true;Min Pool Size=0;Max Pool Size=100;";

        public async Task<T> ExecuteAsync<T>(Func<MySqlConnection, Task<T>> action)
        {
            await using var connection =  new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return await action(connection);
        }

        public async Task ExecuteAsync(Func<MySqlConnection, Task> action)
        {
            await using var connection =  new MySqlConnection(_connectionString);
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
                await using var connection =  new MySqlConnection(_connectionString);
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
                await using var connection =  new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                res = await action(connection);
                if (res != null) await _redis.SaveQuery(key, res);
                return res;
            }
        }

        public async Task<bool> ExecuteInsertAsync(List<string> invalidatedKeys, Func<MySqlConnection, Task<bool>> action)
        {
            await _redis.InvalidateKeys(invalidatedKeys);
            await using var connection =  new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return await action(connection);
        }
    }
}
