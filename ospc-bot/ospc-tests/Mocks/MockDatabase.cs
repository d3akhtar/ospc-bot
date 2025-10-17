using MySql.Data.MySqlClient;

using OSPC.Infrastructure.Database;
using OSPC.Domain.Common;

namespace OSPC.Tests.Mocks
{
    public class MockDatabase : IDatabase
    {
        public Task<T> ExecuteAsync<T>(Func<MySqlConnection, Task<T>> action) => action(new MySqlConnection());
        public Task ExecuteAsync(Func<MySqlConnection, Task> action) => action(new MySqlConnection());
        public Task<Result<T>> ExecuteCommandAsync<T>(string key, Func<MySqlConnection, Task<Result<T>>> action) => action(new MySqlConnection());
        public Task CommitTransactionAsync(MySqlTransaction transaction) => Task.CompletedTask;
        public Task<bool> ExecuteInsertAsync(List<string> invalidatedKeys, Func<MySqlConnection, Task<bool>> action) => action(new MySqlConnection());
    }
}
