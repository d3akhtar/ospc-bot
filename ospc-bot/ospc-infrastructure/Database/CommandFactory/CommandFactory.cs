using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;

using MySql.Data.MySqlClient;
using OSPC.Domain.Common;
using OSPC.Domain.Model;
using OSPC.Parsing.ParsedObjects;

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
            LogCommandCreation(new { UserId = userId, BeatmapId = beatmapId });
            return CreateBeatmapPlaycountCommand("CALL GetPlaycountForBeatmap(@UserId, @BeatmapId)", conn, userId, beatmapId);
        }

        public Result<MySqlCommand> CreateGetBeatmapPlaycountForUserCommand(MySqlConnection conn, int userId, int beatmapId)
        {
            LogCommandCreation(new { UserId = userId, BeatmapId = beatmapId });
            return CreateBeatmapPlaycountCommand("CALL GetBeatmapPlaycountForUser(@UserId, @BeatmapId)", conn, userId, beatmapId);
        }

        private Result<MySqlCommand> CreateBeatmapPlaycountCommand(string query, MySqlConnection conn, int userId, int beatmapId)
        {
            try
            {
                var command = new MySqlCommand(query, conn);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@BeatmapId", beatmapId);
                return command;
            }
            catch (MySqlException ex)
            {
                _logger.LogCritical(ex, "MySql exception occured");
                return ex;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Exception during command creation");
                return ex;
            }
        }

        public Result<MySqlCommand> CreateUpdateReferencedBeatmapIdForChannelCommand(MySqlConnection conn, ulong channelId, int beatmapId)
        {
            LogCommandCreation(new { ChannelId = channelId, BeatmapId = beatmapId });

            try
            {
                string query = "CALL UpdateReferencedBeatmapIdForChannel(@ChannelId, @BeatmapId)";
                var command = new MySqlCommand(query, conn);
                command.Parameters.AddWithValue("@ChannelId", channelId);
                command.Parameters.AddWithValue("@BeatmapId", beatmapId);
                return command;
            }
            catch (MySqlException ex)
            {
                _logger.LogCritical(ex, "MySql exception occured");
                return ex;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Exception during command creation");
                return ex;
            }
        }

        public Result<MySqlCommand> CreateGetReferencedBeatmapIdForChannelCommand(MySqlConnection conn, ulong channelId)
        {
            LogCommandCreation(new { ChannelId = channelId });

            try
            {
                string query = "CALL GetReferencedBeatmapIdForChannel(@ChannelId)";
                var command = new MySqlCommand(query, conn);
                command.Parameters.AddWithValue("@ChannelId", channelId);
                return command;
            }
            catch (MySqlException ex)
            {
                _logger.LogCritical(ex, "MySql exception occured");
                return ex;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Exception during command creation");
                return ex;
            }
        }

        public Result<MySqlCommand> CreateAddDiscordPlayerMappingCommand(MySqlConnection conn, ulong discordUserId, int playerUserId)
        {
            LogCommandCreation(new { DiscordUserId = discordUserId, PlayerUserId = playerUserId });

            try
            {
                string query = "CALL AddDiscordPlayerMapping(@DiscordUserId, @PlayerUserId)";
                var command = new MySqlCommand(query, conn);
                command.Parameters.AddWithValue("@DiscordUserId", discordUserId);
                command.Parameters.AddWithValue("@PlayerUserId", playerUserId);
                return command;
            }
            catch (MySqlException ex)
            {
                _logger.LogCritical(ex, "MySql exception occured");
                return ex;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Exception during command creation");
                return ex;
            }
        }

        public Result<MySqlCommand> CreateGetPlayerInfoFromDiscordIdCommand(MySqlConnection conn, ulong discordUserId)
        {
            LogCommandCreation(new { DiscordUserId = discordUserId });

            try
            {
                string query = "CALL GetPlayerInfoFromDiscordId(@DiscordUserId)";
                var command = new MySqlCommand(query, conn);
                command.Parameters.AddWithValue("@DiscordUserId", discordUserId);
                return command;
            }
            catch (MySqlException ex)
            {
                _logger.LogCritical(ex, "MySql exception occured");
                return ex;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Exception during command creation");
                return ex;
            }
        }

        public Result<MySqlCommand> CreateAddUserCommand(MySqlConnection conn, User user)
        {
            LogCommandCreation(new { User = user });

            try
            {
                string query = "CALL AddUser(@Id, @Username, @CountryCode, @AvatarUrl, @ProfileColour)";
                var command = new MySqlCommand(query, conn);
                command.Parameters.AddWithValue("@Id", user.Id);
                command.Parameters.AddWithValue("@Username", user.Username);
                command.Parameters.AddWithValue("@CountryCode", user.CountryCode);
                command.Parameters.AddWithValue("@AvatarUrl", user.AvatarUrl);
                command.Parameters.AddWithValue("@ProfileColour", user.ProfileColour);
                return command;
            }
            catch (MySqlException ex)
            {
                _logger.LogCritical(ex, "MySql exception occured");
                return ex;
            }
            catch (Exception ex)
            {
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

            try
            {
                string query = "CALL GetUserByUsername(@Username)";
                var command = new MySqlCommand(query, conn);
                command.Parameters.AddWithValue("@Username", username);
                return command;
            }
            catch (MySqlException ex)
            {
                _logger.LogCritical(ex, "MySql exception occured");
                return ex;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Exception during command creation");
                return ex;
            }
        }


        public Result<MySqlCommand> CreateGetUserWithDiscordIdCommand(MySqlConnection conn, ulong discordUserId)
        {
            LogCommandCreation(new { DiscordUserId = discordUserId });

            try
            {
                string query = "CALL GetUserWithDiscordId(@DiscordUserId)";
                var command = new MySqlCommand(query, conn);
                command.Parameters.AddWithValue("@DiscordUserId", discordUserId);
                return command;
            }
            catch (MySqlException ex)
            {
                _logger.LogCritical(ex, "MySql exception occured");
                return ex;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Exception during command creation");
                return ex;
            }
        }

        public Result<MySqlCommand> CreateBeatmapPlaycountFilterCommand(MySqlConnection conn, SearchParams searchParams, int userId, int pageSize, int pageNumber)
        {
            LogCommandCreation(new { SearchParams = searchParams, UserId = userId, PageSize = pageSize, PageNumber = pageNumber });
            return GetNewCommandWithConnection(conn, CreateFilterQuery(searchParams, userId, pageSize, pageNumber));
        }

        public Result<MySqlCommand> CreateBeatmapPlaycountFilterResultCountCommand(MySqlConnection conn, SearchParams searchParams, int userId)
        {
            LogCommandCreation(new { SearchParams = searchParams, UserId = userId });
            return GetNewCommandWithConnection(conn, CreateFilterResultCountQuery(searchParams, userId));
        }

        private Result<MySqlCommand> GetNewCommandWithConnection(MySqlConnection conn, MySqlCommand command)
        {            
            try
            {
                command.Connection = conn;
                return command;
            }
            catch (MySqlException ex)
            {
                _logger.LogCritical(ex, "MySql exception occured");
                return ex;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Exception during command creation");
                return ex;
            }
        }

        private Result<MySqlCommand> CreateQueryIdCommand(string query, MySqlConnection conn, int id)
        {
            try
            {
                var command = new MySqlCommand(query, conn);
                command.Parameters.AddWithValue("@Id", id);
                return command;
            }
            catch (MySqlException ex)
            {
                _logger.LogCritical(ex, "MySql exception occured");
                return ex;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Exception during command creation");
                return ex;
            }
        }

        private MySqlCommand CreateFilterQuery(SearchParams searchParams, int userId, int pageSize, int pageNumber)
            => searchParams.GetFilterQueryCommand(userId, pageSize, pageNumber);

        private MySqlCommand CreateFilterResultCountQuery(SearchParams searchParams, int userId)
            => searchParams.GetFilterResultCountQueryCommand(userId);

        private void LogCommandCreation(object arguments, [CallerMemberName] string? methodName = null)
            => _logger.LogDebug("Creating command from method: {MethodName} with arguments: {@Arguments}", methodName, arguments);
    }
}
