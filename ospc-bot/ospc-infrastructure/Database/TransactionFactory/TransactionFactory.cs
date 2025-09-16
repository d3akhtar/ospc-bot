using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using OSPC.Domain.Model;
using OSPC.Utils;

namespace OSPC.Infrastructure.Database.TransactionFactory
{
	public class TransactionFactory : ITransactionFactory
	{
        private readonly ILogger<TransactionFactory> _logger;

        public TransactionFactory(ILogger<TransactionFactory> logger)
		{
			_logger = logger;
		}
		
		public async Task<Result<MySqlTransaction>> CreateAddBeatmapPlaycountTransaction(MySqlConnection conn, IEnumerable<BeatmapPlaycount> bpcs)
		{
			LogTransactionCreation(new { BeatmapPlaycountBeatmapIds = bpcs.Select(bpc => bpc.BeatmapId) } );

			var transaction = await conn.BeginTransactionAsync(); 
			string query = "CALL AddBeatmapPlaycount(@UserId, @BeatmapId, @Count)";
			var command = new MySqlCommand(query, conn, transaction);

			try {
	            command.Parameters.Add("@UserId", MySqlDbType.Int32);
	            command.Parameters.Add("@BeatmapId", MySqlDbType.Int32);
	            command.Parameters.Add("@Count", MySqlDbType.Int32);

				foreach (BeatmapPlaycount bpc in bpcs) {
	                command.Parameters["@UserId"].Value = bpc.UserId;
	                command.Parameters["@BeatmapId"].Value = bpc.BeatmapId;
	                command.Parameters["@Count"].Value = bpc.Count;
	                await command.ExecuteNonQueryAsync();
				}
			} catch(MySqlException ex) {
				_logger.LogCritical("MySql exception occured");
				return ex;
			} catch (Exception ex) {
				_logger.LogCritical(ex, "Exception while creating transaction");
				return ex;
			}

			return transaction;
		}

		public async Task<Result<MySqlTransaction>> CreateAddBeatmapTransaction(MySqlConnection conn, IEnumerable<Beatmap> beatmaps)
		{
			LogTransactionCreation(new { BeatmapIds = beatmaps.Select(beatmap => beatmap.Id )});
			
			var transaction = await conn.BeginTransactionAsync();			
            string query = "CALL AddBeatmap(@Id, @Version, @DifficultyRating, @BeatmapSetId, @CircleSize, @BPM, @Length, @HpDrain, @OD, @AR)";
			var command = new MySqlCommand(query, conn,transaction);
			
			try {
	            command.Parameters.Add("@Id", MySqlDbType.Int32);
	            command.Parameters.Add("@Version", MySqlDbType.VarChar);
	            command.Parameters.Add("@DifficultyRating", MySqlDbType.Float);
	            command.Parameters.Add("@BeatmapSetId", MySqlDbType.Int32);
	            command.Parameters.Add("@CircleSize", MySqlDbType.Float);
	            command.Parameters.Add("@BPM", MySqlDbType.Float);
	            command.Parameters.Add("@Length", MySqlDbType.Float);
	            command.Parameters.Add("@HpDrain", MySqlDbType.Float);
	            command.Parameters.Add("@OD", MySqlDbType.Float);
	            command.Parameters.Add("@AR", MySqlDbType.Float);

	            foreach (Beatmap beatmap in beatmaps){
	                command.Parameters["@Id"].Value = beatmap.Id;
	                command.Parameters["@Version"].Value = beatmap.Version;
	                command.Parameters["@DifficultyRating"].Value = beatmap.DifficultyRating;
	                command.Parameters["@BeatmapSetId"].Value = beatmap.BeatmapSetId;
	                command.Parameters["@CircleSize"].Value = beatmap.CircleSize;
	                command.Parameters["@BPM"].Value = beatmap.BPM;
	                command.Parameters["@Length"].Value = beatmap.Length;
	                command.Parameters["@HpDrain"].Value = beatmap.HpDrain;
	                command.Parameters["@OD"].Value = beatmap.DifficultyRating;
	                command.Parameters["@AR"].Value = beatmap.AR;
	                await command.ExecuteNonQueryAsync();
	            }
			} catch(MySqlException ex) {
				_logger.LogCritical("MySql exception occured");
				return ex;
			} catch (Exception ex) {
				_logger.LogCritical(ex, "Exception while creating transaction");
				return ex;
			}

			return transaction;
		}

		public async Task<Result<MySqlTransaction>> CreateAddBeatmapSetTransaction(MySqlConnection conn, IEnumerable<BeatmapSet> beatmapSets)
		{
			LogTransactionCreation(new { BeatmapSetIds = beatmapSets.Select(beatmapSet => beatmapSet.Id) });
			
			var transaction = await conn.BeginTransactionAsync();
			string query = "CALL AddBeatmapSet(@Id, @Artist, @Title, @SlimCover2x, @UserId)";
			var command = new MySqlCommand(query, conn, transaction);

			try {
	            command.Parameters.Add("@Id", MySqlDbType.Int32);
	            command.Parameters.Add("@Artist", MySqlDbType.VarChar);
	            command.Parameters.Add("@Title", MySqlDbType.VarChar);
	            command.Parameters.Add("@SlimCover2x", MySqlDbType.VarChar);
	            command.Parameters.Add("@UserId", MySqlDbType.Int32);

	            foreach (BeatmapSet set in beatmapSets){
	                command.Parameters["@Id"].Value = set.Id;
	                command.Parameters["@Artist"].Value = set.Artist;
	                command.Parameters["@Title"].Value = set.Title;
	                command.Parameters["@SlimCover2x"].Value = set.Covers.SlimCover2x;
	                command.Parameters["@UserId"].Value = set.UserId;
	                await command.ExecuteNonQueryAsync();
	            }
			} catch(MySqlException ex) {
				_logger.LogCritical("MySql exception occured");
				return ex;
			} catch (Exception ex) {
				_logger.LogCritical(ex, "Exception while creating transaction");
				return ex;
			}

			return transaction;
		}

		private void LogTransactionCreation(object arguments, [CallerMemberName] string? methodName = null)
			=> _logger.LogDebug("Creating transaction from method: {MethodName} with arguments: {@Arguments}", methodName, arguments);
	}
}
