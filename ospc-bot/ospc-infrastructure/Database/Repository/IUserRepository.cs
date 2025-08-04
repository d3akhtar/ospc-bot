using OSPC.Domain.Model;

namespace OSPC.Infrastructure.Database.Repository
{
    public interface IUserRepository
    {
        public Task<User?> GetUserById(int id);
        public Task<bool> AddUser(User user);
        public Task<User?> GetUserByUsername(string username);
        public Task<bool> AddDiscordPlayerMapping(ulong discordUserId, int playerUserId);
        public Task<DiscordPlayer?> GetPlayerInfoFromDiscordId(ulong discordUserId);
        public Task<User?> GetUserWithDiscordId(ulong discordUserId);
    }
}
