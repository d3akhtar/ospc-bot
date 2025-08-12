using MySql.Data.MySqlClient;
using OSPC.Domain.Model;

namespace OSPC.Infrastructure.Database.TransactionFactory
{
	public interface ITransactionFactory
	{
		public Task<MySqlTransaction> CreateAddBeatmapPlaycountTransaction(MySqlConnection conn, IEnumerable<BeatmapPlaycount> bpcs);
		public Task<MySqlTransaction> CreateAddBeatmapTransaction(MySqlConnection conn, IEnumerable<Beatmap> beatmaps);
		public Task<MySqlTransaction> CreateAddBeatmapSetTransaction(MySqlConnection conn, IEnumerable<BeatmapSet> beatmapSets);
	}
}
