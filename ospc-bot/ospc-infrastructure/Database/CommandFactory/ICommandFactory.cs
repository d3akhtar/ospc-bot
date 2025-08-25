using MySql.Data.MySqlClient;
using OSPC.Domain.Model;
using OSPC.Utils.Parsing;

namespace OSPC.Infrastructure.Database.CommandFactory
{
	public interface ICommandFactory
	{		
		public MySqlCommand CreateGetBeatmapByIdCommand(MySqlConnection conn, int beatmapId);
		public MySqlCommand CreateGetBeatmapSetByIdCommand(MySqlConnection conn, int beatmapSetId);	
		public MySqlCommand CreateGetPlaycountForBeatmapCommand(MySqlConnection conn, int userId, int beatmapId);
		public MySqlCommand CreateGetBeatmapPlaycountForUserCommand(MySqlConnection conn, int userId, int beatmapId);
		public MySqlCommand CreateUpdateReferencedBeatmapIdForChannelCommand(MySqlConnection conn, ulong channelId, int beatmapId);
		public MySqlCommand CreateGetReferencedBeatmapIdForChannelCommand(MySqlConnection conn, ulong channelId);
		public MySqlCommand CreateAddDiscordPlayerMappingCommand(MySqlConnection conn, ulong discordUserId, int playerUserId);
		public MySqlCommand CreateGetPlayerInfoFromDiscordIdCommand(MySqlConnection conn, ulong discordUserId);
		public MySqlCommand CreateAddUserCommand(MySqlConnection conn, User user);
		public MySqlCommand CreateGetUserByIdCommand(MySqlConnection conn, int userId);
		public MySqlCommand CreateGetUserByUsernameCommand(MySqlConnection conn, string username);
		public MySqlCommand CreateGetUserWithDiscordIdCommand(MySqlConnection conn, ulong discordUserId);
		public MySqlCommand CreateBeatmapPlaycountFilterCommand(MySqlConnection conn, SearchParams searchParams, int userId, int pageSize = -1, int pageNumber = -1);
	}
}
