using MySql.Data.MySqlClient;

using OSPC.Domain.Model;
using OSPC.Utils;
using OSPC.Utils.Parsing;

namespace OSPC.Infrastructure.Database.CommandFactory
{
    public interface ICommandFactory
    {
        public Result<MySqlCommand> CreateGetBeatmapByIdCommand(MySqlConnection conn, int beatmapId);
        public Result<MySqlCommand> CreateGetBeatmapSetByIdCommand(MySqlConnection conn, int beatmapSetId);
        public Result<MySqlCommand> CreateGetPlaycountForBeatmapCommand(MySqlConnection conn, int userId, int beatmapId);
        public Result<MySqlCommand> CreateGetBeatmapPlaycountForUserCommand(MySqlConnection conn, int userId, int beatmapId);
        public Result<MySqlCommand> CreateUpdateReferencedBeatmapIdForChannelCommand(MySqlConnection conn, ulong channelId, int beatmapId);
        public Result<MySqlCommand> CreateGetReferencedBeatmapIdForChannelCommand(MySqlConnection conn, ulong channelId);
        public Result<MySqlCommand> CreateAddDiscordPlayerMappingCommand(MySqlConnection conn, ulong discordUserId, int playerUserId);
        public Result<MySqlCommand> CreateGetPlayerInfoFromDiscordIdCommand(MySqlConnection conn, ulong discordUserId);
        public Result<MySqlCommand> CreateAddUserCommand(MySqlConnection conn, User user);
        public Result<MySqlCommand> CreateGetUserByIdCommand(MySqlConnection conn, int userId);
        public Result<MySqlCommand> CreateGetUserByUsernameCommand(MySqlConnection conn, string username);
        public Result<MySqlCommand> CreateGetUserWithDiscordIdCommand(MySqlConnection conn, ulong discordUserId);
        public Result<MySqlCommand> CreateBeatmapPlaycountFilterCommand(MySqlConnection conn, SearchParams searchParams, int userId, int pageSize, int pageNumber);
        public Result<MySqlCommand> CreateBeatmapPlaycountFilterResultCountCommand(MySqlConnection conn, SearchParams searchParams, int userId);
    }
}
