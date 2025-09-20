using Microsoft.Extensions.Logging;

using OSPC.Domain.Model;
using OSPC.Infrastructure.Database.CommandFactory;
using OSPC.Utils;
using OSPC.Utils.Cache;

namespace OSPC.Infrastructure.Database.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;
        private readonly ICommandFactory _commandFactory;
        private readonly IDatabase _db;

        public UserRepository(ILogger<UserRepository> logger, IDatabase db, ICommandFactory commandFactory)
        {
            _logger = logger;
            _commandFactory = commandFactory;
            _db = db;
        }

        public async Task<bool> AddDiscordPlayerMapping(ulong discordUserId, int playerUserId)
        {
            _logger.LogDebug("Adding discord player mapping with discordUserId: {DiscordUserId}, playerUserId: {PlayerUserId}", discordUserId, playerUserId);

            List<string> invalidatedKeys =
            [
                CacheKey.ConvertTypeToKey<DiscordPlayer>((discordUserId, "discId")),
                CacheKey.ConvertTypeToKey<User>((discordUserId, "discId"))
            ];
            return await _db.ExecuteInsertAsync(
                invalidatedKeys,
                async (conn) =>
                {
                    var result = _commandFactory.CreateAddDiscordPlayerMappingCommand(conn, discordUserId, playerUserId);
                    if (!result.Successful)
                    {
                        _logger.LogError("Error occured during insert {@Error}", result.Error!);
                        return false;
                    }

                    using var command = result.Value!;
                    return await command.ExecuteNonQueryAsync() > 0;
                });
        }

        public async Task<Result<DiscordPlayer>> GetPlayerInfoFromDiscordId(ulong discordUserId)
        {
            _logger.LogDebug("Getting player info with discordUserId: {DiscordUserId}", discordUserId);

            string key = CacheKey.ConvertTypeToKey<DiscordPlayer>((discordUserId, "discId"));
            return await _db.ExecuteCommandAsync<DiscordPlayer>(key, async (conn) =>
            {
                var result = _commandFactory.CreateGetPlayerInfoFromDiscordIdCommand(conn, discordUserId);
                if (result.Successful)
                {
                    using var command = result.Value!;
                    using var reader = await command.ExecuteReaderAsync();
                    return reader.Read() ? reader.ReadDiscordPlayerMapping() : Errors.NotFound("Couldn't find osu user linked to your discord profile");
                }
                else
                    return result.Error!;
            });
        }

        public async Task<bool> AddUser(User user)
        {
            _logger.LogDebug("Adding user: {@User}", user);

            return await _db.ExecuteAsync<bool>(async (conn) =>
            {
                var result = _commandFactory.CreateAddUserCommand(conn, user);
                if (!result.Successful)
                {
                    _logger.LogError("Error occured during insert {@Error}", result.Error!);
                    return false;
                }

                using var command = result.Value!;
                return await command.ExecuteNonQueryAsync() > 0;
            });
        }

        public async Task<Result<User>> GetUserById(int id)
        {
            _logger.LogDebug("Getting user with id: {UserId}", id);

            string key = CacheKey.ConvertTypeToKey<User>((id, "osuid"));
            return await _db.ExecuteCommandAsync<User>(key, async (conn) =>
            {
                var result = _commandFactory.CreateGetUserByIdCommand(conn, id);
                if (result.Successful)
                {
                    using var command = result.Value!;
                    using var reader = await command.ExecuteReaderAsync();
                    return reader.Read() ? reader.ReadUser() : Errors.NotFound($"Couldn't find user with id: {id}");
                }
                else
                    return result.Error!;
            });
        }

        public async Task<Result<User>> GetUserByUsername(string username)
        {
            _logger.LogDebug("Getting user info for {Username}", username);

            string key = CacheKey.ConvertTypeToKey<User>((username, "osuign"));
            return await _db.ExecuteCommandAsync<User>(key, async (conn) =>
            {
                var result = _commandFactory.CreateGetUserByUsernameCommand(conn, username);
                if (result.Successful)
                {
                    using var command = result.Value!;
                    using var reader = await command.ExecuteReaderAsync();
                    return reader.Read() ? reader.ReadUser() : Errors.NotFound($"Couldn't find user with username: {username}");
                }
                else
                    return result.Error!;
            });
        }

        public async Task<Result<User>> GetUserWithDiscordId(ulong discordUserId)
        {
            _logger.LogDebug("Getting user with discordUserId: {DiscordUserId}", discordUserId);

            string key = CacheKey.ConvertTypeToKey<User>((discordUserId, "discId"));
            return await _db.ExecuteCommandAsync<User>(key, async (conn) =>
            {
                var result = _commandFactory.CreateGetUserWithDiscordIdCommand(conn, discordUserId);
                if (result.Successful)
                {
                    using var command = result.Value!;
                    using var reader = await command.ExecuteReaderAsync();
                    return reader.Read() ? reader.ReadUser() : Errors.NotFound($"Couldn't find user linked to discord profile");
                }
                else
                    return result.Error!;
            });
        }
    }
}
