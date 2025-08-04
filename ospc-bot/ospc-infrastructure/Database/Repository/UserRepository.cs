using OSPC.Domain.Model;
using OSPC.Utils;

namespace OSPC.Infrastructure.Database.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DbContext _db;
        public UserRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<bool> AddDiscordPlayerMapping(ulong discordUserId, int playerUserId)
        {
            List<string> invalidatedKeys =
            [
                CacheKey.ConvertTypeToKey<DiscordPlayer>((discordUserId, "discId")),
                CacheKey.ConvertTypeToKey<User>((discordUserId, "discId"))
            ];
            return await _db.ExecuteInsertAsync(
                invalidatedKeys,
                async conn
                    => await CommandFactory.CreateAddDiscordPlayerMappingCommand(conn, discordUserId, playerUserId).ExecuteNonQueryAsync() > 0);
        }

        public async Task<DiscordPlayer?> GetPlayerInfoFromDiscordId(ulong discordUserId)
        {
            string key = CacheKey.ConvertTypeToKey<DiscordPlayer>((discordUserId, "discId"));
            return await _db.ExecuteCommandAsync<DiscordPlayer?>(key, async (conn) =>
            {
                using var reader = await CommandFactory.CreateGetPlayerInfoFromDiscordIdCommand(conn, discordUserId).ExecuteReaderAsync();
                return reader.Read() ? reader.ReadDiscordPlayerMapping():null;
            });
        }

        public async Task<bool> AddUser(User user)
            => await _db.ExecuteAsync<bool>(async conn
                => await CommandFactory.CreateAddUserCommand(conn, user).ExecuteNonQueryAsync() > 0);

        public async Task<User?> GetUserById(int id)
        {
            string key = CacheKey.ConvertTypeToKey<User>((id, "osuid"));
            return await _db.ExecuteCommandAsync<User?>(key, async (conn) =>
            {
                using var reader = await CommandFactory.CreateGetUserByIdCommand(conn, id).ExecuteReaderAsync();
                return reader.Read() ? reader.ReadUser():null;
            });
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            string key = CacheKey.ConvertTypeToKey<User>((username, "osuign"));
            return await _db.ExecuteCommandAsync<User?>(key, async (conn) =>
            {
                using var reader = await CommandFactory.CreateGetUserByUsernameCommand(conn, username).ExecuteReaderAsync();
                return reader.Read() ? reader.ReadUser():null;
            });
        }

        public async Task<User?> GetUserWithDiscordId(ulong discordUserId)
        {
            string key = CacheKey.ConvertTypeToKey<User>((discordUserId, "discId"));
            return await _db.ExecuteCommandAsync<User?>(key, async (conn) =>
            {
                using var reader = await CommandFactory.CreateGetUserWithDiscordIdCommand(conn, discordUserId).ExecuteReaderAsync();
                return reader.Read() ? reader.ReadUser():null;
            });
        }
    }
}
