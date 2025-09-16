using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using OSPC.Domain.Model;
using OSPC.Utils;
using OSPC.Utils.Parsing;

namespace OSPC.Infrastructure.Database.CommandFactory
{
	public class CommandFactory : ICommandFactory
	{
        private readonly ILogger<CommandFactory> _logger;

        public CommandFactory(ILogger<CommandFactory> logger)
		{
			_logger = logger;
		}
		
		public Result<MySqlCommand> CreateGetBeatmapByIdCommand(MySqlConnection conn, int beatmapId)
		{
			LogCommandCreation(new { BeatmapId = beatmapId });
			return CreateQueryIdCommand("CALL GetBeatmapById(@Id)", conn, beatmapId);
		}

		public Result<MySqlCommand> CreateGetBeatmapSetByIdCommand(MySqlConnection conn, int beatmapSetId)
		{
			LogCommandCreation(new { BeatmapSetId = beatmapSetId });
			return CreateQueryIdCommand("CALL GetBeatmapSetById(@Id)", conn, beatmapSetId);
		}

		public Result<MySqlCommand> CreateGetPlaycountForBeatmapCommand(MySqlConnection conn, int userId, int beatmapId)
		{
			LogCommandCreation(new { UserId = userId, BeatmapId = beatmapId} );
			return CreateBeatmapPlaycountCommand("CALL GetPlaycountForBeatmap(@UserId, @BeatmapId)", conn, userId, beatmapId);
		}

		public Result<MySqlCommand> CreateGetBeatmapPlaycountForUserCommand(MySqlConnection conn, int userId, int beatmapId)
		{
			LogCommandCreation(new { UserId = userId, BeatmapId = beatmapId} );
			return CreateBeatmapPlaycountCommand("CALL GetBeatmapPlaycountForUser(@UserId, @BeatmapId)", conn, userId, beatmapId);
		}

		private Result<MySqlCommand> CreateBeatmapPlaycountCommand(string query, MySqlConnection conn, int userId, int beatmapId)
		{
			try {
				var command = new MySqlCommand(query, conn);
				command.Parameters.AddWithValue("@UserId", userId);
				command.Parameters.AddWithValue("@BeatmapId", beatmapId);
				return command;
			} catch (MySqlException ex) {
				_logger.LogCritical(ex, "MySql exception occured");
				return ex;
			} catch (Exception ex) {
				_logger.LogCritical(ex, "Exception during command creation");
				return ex;
			}
		}

		public Result<MySqlCommand> CreateUpdateReferencedBeatmapIdForChannelCommand(MySqlConnection conn, ulong channelId, int beatmapId)
		{
			LogCommandCreation(new { ChannelId = channelId, BeatmapId = beatmapId });
			
			try {
				string query = "CALL UpdateReferencedBeatmapIdForChannel(@ChannelId, @BeatmapId)";
				var command = new MySqlCommand(query, conn);
				command.Parameters.AddWithValue("@ChannelId", channelId);
				command.Parameters.AddWithValue("@BeatmapId", beatmapId);
				return command;
			} catch (MySqlException ex) {
				_logger.LogCritical(ex, "MySql exception occured");
				return ex;
			} catch (Exception ex) {
				_logger.LogCritical(ex, "Exception during command creation");
				return ex;
			}
		}
		
		public Result<MySqlCommand> CreateGetReferencedBeatmapIdForChannelCommand(MySqlConnection conn, ulong channelId)
		{
			LogCommandCreation(new { ChannelId = channelId });
			
			try {
				string query = "CALL GetReferencedBeatmapIdForChannel(@ChannelId)";
				var command = new MySqlCommand(query, conn);
				command.Parameters.AddWithValue("@ChannelId", channelId);
				return command;
			} catch (MySqlException ex) {
				_logger.LogCritical(ex, "MySql exception occured");
				return ex;
			} catch (Exception ex) {
				_logger.LogCritical(ex, "Exception during command creation");
				return ex;
			}
		}

		public Result<MySqlCommand> CreateAddDiscordPlayerMappingCommand(MySqlConnection conn, ulong discordUserId, int playerUserId)
		{
			LogCommandCreation(new { DiscordUserId = discordUserId, PlayerUserId = playerUserId });
			
			try {
				string query = "CALL AddDiscordPlayerMapping(@DiscordUserId, @PlayerUserId)";
				var command = new MySqlCommand(query, conn);
				command.Parameters.AddWithValue("@DiscordUserId", discordUserId);
				command.Parameters.AddWithValue("@PlayerUserId", playerUserId);
				return command;
			} catch (MySqlException ex) {
				_logger.LogCritical(ex, "MySql exception occured");
				return ex;
			} catch (Exception ex) {
				_logger.LogCritical(ex, "Exception during command creation");
				return ex;
			}
		}

		public Result<MySqlCommand> CreateGetPlayerInfoFromDiscordIdCommand(MySqlConnection conn, ulong discordUserId)
		{
			LogCommandCreation(new { DiscordUserId = discordUserId });
			
			try {
				string query = "CALL GetPlayerInfoFromDiscordId(@DiscordUserId)";
				var command = new MySqlCommand(query, conn);
				command.Parameters.AddWithValue("@DiscordUserId", discordUserId);
				return command;
			} catch (MySqlException ex) {
				_logger.LogCritical(ex, "MySql exception occured");
				return ex;
			} catch (Exception ex) {
				_logger.LogCritical(ex, "Exception during command creation");
				return ex;
			}
		}

		public Result<MySqlCommand> CreateAddUserCommand(MySqlConnection conn, User user)
		{
			LogCommandCreation(new { User = user });

			try {
				string query = "CALL AddUser(@Id, @Username, @CountryCode, @AvatarUrl, @ProfileColour)";
				var command = new MySqlCommand(query, conn);
				command.Parameters.AddWithValue("@Id", user.Id);
				command.Parameters.AddWithValue("@Username", user.Username);
				command.Parameters.AddWithValue("@CountryCode", user.CountryCode);
				command.Parameters.AddWithValue("@AvatarUrl", user.AvatarUrl);
				command.Parameters.AddWithValue("@ProfileColour", user.ProfileColour);
				return command;
			} catch (MySqlException ex) {
				_logger.LogCritical(ex, "MySql exception occured");
				return ex;
			} catch (Exception ex) {
				_logger.LogCritical(ex, "Exception during command creation");
				return ex;
			}
		}

		public Result<MySqlCommand> CreateGetUserByIdCommand(MySqlConnection conn, int userId)
		{
			LogCommandCreation(new { UserId = userId });
			return CreateQueryIdCommand("CALL GetUserById(@Id)", conn, userId);
		}


		public Result<MySqlCommand> CreateGetUserByUsernameCommand(MySqlConnection conn, string username)
		{
			LogCommandCreation(new { Username = username });
			
			try {
				string query = "CALL GetUserByUsername(@Username)";
				var command = new MySqlCommand(query, conn);
				command.Parameters.AddWithValue("@Username", username);
				return command;
			} catch (MySqlException ex) {
				_logger.LogCritical(ex, "MySql exception occured");
				return ex;
			} catch (Exception ex) {
				_logger.LogCritical(ex, "Exception during command creation");
				return ex;
			}
		}


		public Result<MySqlCommand> CreateGetUserWithDiscordIdCommand(MySqlConnection conn, ulong discordUserId)
		{
			LogCommandCreation(new { DiscordUserId = discordUserId });

			try {
				string query = "CALL GetUserWithDiscordId(@DiscordUserId)";
				var command = new MySqlCommand(query, conn);
				command.Parameters.AddWithValue("@DiscordUserId", discordUserId);
				return command;
			} catch (MySqlException ex) {
				_logger.LogCritical(ex, "MySql exception occured");
				return ex;
			} catch (Exception ex) {
				_logger.LogCritical(ex, "Exception during command creation");
				return ex;
			}
		}

		public Result<MySqlCommand> CreateBeatmapPlaycountFilterCommand(MySqlConnection conn, SearchParams searchParams, int userId, int pageSize = -1, int pageNumber = -1)
        {
			LogCommandCreation(new { SearchParams = searchParams, UserId = userId, PageSize = pageSize, PageNumber = pageNumber });
			
			try {
	            (string query, bool generalQuery) = CreateFilterQuery(searchParams, userId, pageSize, pageNumber);
	            MySqlCommand command = new MySqlCommand(query, conn);
	            if (generalQuery) command.Parameters.AddWithValue("@query", searchParams.Query);
	            else {
	                command.Parameters.AddWithValue("@artist", searchParams.Artist ?? string.Empty);
	                command.Parameters.AddWithValue("@title", searchParams.Title ?? string.Empty);
	            }
	            return command;
			} catch (MySqlException ex) {
				_logger.LogCritical(ex, "MySql exception occured");
				return ex;
			} catch (Exception ex) {
				_logger.LogCritical(ex, "Exception during command creation");
				return ex;
			}
        }
		
		private Result<MySqlCommand> CreateQueryIdCommand(string query, MySqlConnection conn, int id)
		{
			try {
				var command = new MySqlCommand(query, conn);
				command.Parameters.AddWithValue("@Id", id);
				return command;
			} catch (MySqlException ex) {
				_logger.LogCritical(ex, "MySql exception occured");
				return ex;
			} catch (Exception ex) {
				_logger.LogCritical(ex, "Exception during command creation");
				return ex;
			}
		}

        private (string, bool) CreateFilterQuery(SearchParams searchParams, int userId, int pageSize, int pageNumber) 
        {
            bool countQuery = pageSize == -1 || pageNumber == -1;
            bool generalQuery = string.IsNullOrEmpty(searchParams.Artist) && string.IsNullOrEmpty(searchParams.Title);
            string artistConcat = generalQuery ? (searchParams.Exact && searchParams.Query != "" ? "":"%") : (searchParams.Exact && searchParams.Artist != "" ? "":"%");
            string titleConcat = generalQuery ? (searchParams.Exact && searchParams.Query != "" ? "":"%") : (searchParams.Exact && searchParams.Title != "" ? "":"%");
            string countComparison = ComparisonConverter.CreateComparisonClause(searchParams.Playcount!, "BeatmapPlaycounts", "Count");
            string query = $@"
                    SELECT {(countQuery ? "COUNT(BeatmapPlaycounts.Count)":"BeatmapPlaycounts.*,Beatmaps.Id,Beatmaps.Version,Beatmaps.DifficultyRating,Beatmaps.BeatmapSetId,BeatmapSet.*")}
                    FROM BeatmapPlaycounts
                    JOIN Beatmaps ON BeatmapPlaycounts.BeatmapId = Beatmaps.Id 
                    JOIN BeatmapSet ON Beatmaps.BeatmapSetId = BeatmapSet.Id 
                    WHERE BeatmapPlaycounts.UserId = {userId}
                    {countComparison}
                    AND (
                        BeatmapSet.Title LIKE CONCAT('{titleConcat}', {(generalQuery ? "@query":"@title")}, '{titleConcat}') 
                        {(generalQuery ? "OR":"AND")} 
                        BeatmapSet.Artist LIKE CONCAT('{artistConcat}', {(generalQuery ? "@query":"@artist")}, '{artistConcat}')
                    )
                    {(searchParams.BeatmapFilter == null ? "":searchParams.BeatmapFilter.GetClause())}
                    {(
                        countQuery ? "":
                        $@"
                            ORDER BY BeatmapPlaycounts.Count DESC
                            LIMIT {pageSize}
                            OFFSET {(pageNumber-1) * pageSize}
                        "
                    )}
                ";
			
            return (query, generalQuery);
        }

		private void LogCommandCreation(object arguments, [CallerMemberName] string? methodName = null)
			=> _logger.LogDebug("Creating command from method: {MethodName} with arguments: {@Arguments}", methodName, arguments);
	}
}
