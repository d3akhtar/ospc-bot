using OSPC.Domain.Model;
using OSPC.Utils;

namespace OSPC.Infrastructure.Database.Repository
{
    public interface IUserRepository
    {
        public Task<Result<User>> GetUserById(int id);
        public Task<bool> AddUser(User user);
        public Task<Result<User>> GetUserByUsername(string username);
        public Task<bool> AddDiscordPlayerMapping(ulong discordUserId, int playerUserId);
        public Task<Result<DiscordPlayer>> GetPlayerInfoFromDiscordId(ulong discordUserId);
        public Task<Result<User>> GetUserWithDiscordId(ulong discordUserId);
    }
}
