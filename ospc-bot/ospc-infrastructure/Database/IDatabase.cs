using MySql.Data.MySqlClient;

namespace OSPC.Infrastructure.Database
{
	public interface IDatabase
	{
		public Task<T> ExecuteAsync<T>(Func<MySqlConnection, Task<T>> action);
        public Task ExecuteAsync(Func<MySqlConnection, Task> action);
        public Task<T> ExecuteCommandAsync<T>(string key, Func<MySqlConnection, Task<T>> action);
		public Task CommitTransactionAsync(MySqlTransaction transaction);
        public Task<bool> ExecuteInsertAsync(List<string> invalidatedKeys, Func<MySqlConnection, Task<bool>> action);
	}
}
