using MySql.Data.MySqlClient;
using OSPC.Utils;

namespace OSPC.Infrastructure.Database
{
    public interface IDatabase
    {
        public Task<T> ExecuteAsync<T>(Func<MySqlConnection, Task<T>> action);
        public Task ExecuteAsync(Func<MySqlConnection, Task> action);
        public Task<Result<T>> ExecuteCommandAsync<T>(string key, Func<MySqlConnection, Task<Result<T>>> action);
        public Task CommitTransactionAsync(MySqlTransaction transaction);
        public Task<bool> ExecuteInsertAsync(List<string> invalidatedKeys, Func<MySqlConnection, Task<bool>> action);
    }
}
