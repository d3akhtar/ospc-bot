using MySql.Data.MySqlClient;
using OSPC.Domain.Common;
using OSPC.Domain.Model;

namespace OSPC.Infrastructure.Database.TransactionFactory
{
    public interface ITransactionFactory
    {
        public Task<Result<MySqlTransaction>> CreateAddBeatmapPlaycountTransaction(MySqlConnection conn, IEnumerable<BeatmapPlaycount> bpcs);
        public Task<Result<MySqlTransaction>> CreateAddBeatmapTransaction(MySqlConnection conn, IEnumerable<Beatmap> beatmaps);
        public Task<Result<MySqlTransaction>> CreateAddBeatmapSetTransaction(MySqlConnection conn, IEnumerable<BeatmapSet> beatmapSets);
    }
}