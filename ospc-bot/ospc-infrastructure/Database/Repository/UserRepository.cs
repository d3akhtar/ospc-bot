using Microsoft.Extensions.Logging;
using OSPC.Domain.Model;
using OSPC.Infrastructure.Database.CommandFactory;
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
                    var command = _commandFactory.CreateAddDiscordPlayerMappingCommand(conn, discordUserId, playerUserId);
                    return command is {} c && (await c.ExecuteNonQueryAsync() > 0);
                });
        }

        public async Task<DiscordPlayer?> GetPlayerInfoFromDiscordId(ulong discordUserId)
        {
            _logger.LogDebug("Getting player info with discordUserId: {DiscordUserId}", discordUserId);
            
            string key = CacheKey.ConvertTypeToKey<DiscordPlayer>((discordUserId, "discId"));
            return await _db.ExecuteCommandAsync<DiscordPlayer?>(key, async (conn) =>
            {
                var command = _commandFactory.CreateGetPlayerInfoFromDiscordIdCommand(conn, discordUserId);
                if (command is {} c ) {
                    using var reader = await c.ExecuteReaderAsync();
                    return reader.Read() ? reader.ReadDiscordPlayerMapping():null;
                } else return default;
            });
        }

        public async Task<bool> AddUser(User user)
        {
            _logger.LogDebug("Adding user: {@User}", user);
            
            return await _db.ExecuteAsync<bool>(async (conn) =>
            {
                var command = _commandFactory.CreateAddUserCommand(conn, user);
                return command is {} c && (await c.ExecuteNonQueryAsync()) > 0;
            });
        }

        public async Task<User?> GetUserById(int id)
        {
            _logger.LogDebug("Getting user with id: {UserId}", id);
            
            string key = CacheKey.ConvertTypeToKey<User>((id, "osuid"));
            return await _db.ExecuteCommandAsync<User?>(key, async (conn) =>
            {
                using var command = _commandFactory.CreateGetUserByIdCommand(conn, id);
                if (command is {} c) {
                    using var reader = await c.ExecuteReaderAsync();
                    return reader.Read() ? reader.ReadUser():null;
                } else return default;
            });
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            _logger.LogDebug("Getting user info for {Username}", username);
            
            string key = CacheKey.ConvertTypeToKey<User>((username, "osuign"));
            return await _db.ExecuteCommandAsync<User?>(key, async (conn) =>
            {
                var command = _commandFactory.CreateGetUserByUsernameCommand(conn, username);
                if (command is {} c) {
                    using var reader = await c.ExecuteReaderAsync();
                    return reader.Read() ? reader.ReadUser():null;
                } else return default;
            });
        }

        public async Task<User?> GetUserWithDiscordId(ulong discordUserId)
        {
            _logger.LogDebug("Getting user with discordUserId: {DiscordUserId}", discordUserId);
            
            string key = CacheKey.ConvertTypeToKey<User>((discordUserId, "discId"));
            return await _db.ExecuteCommandAsync<User?>(key, async (conn) =>
            {
                var command = _commandFactory.CreateGetUserWithDiscordIdCommand(conn, discordUserId);
                if (command is {} c) {
                    using var reader = await c.ExecuteReaderAsync();
                    return reader.Read() ? reader.ReadUser():null;
                } else return default;
            });
        }
    }
}
