using Discord.Commands;
using Discord.Interactions;
using OSPC.Domain.Model;
using OSPC.Infrastructure.Database.Repository;
using OSPC.Infrastructure.Http;
using OSPC.Utils;

namespace OSPC.Bot.Helper
{
    public static class UserSearch
    {
        public static async Task<User?> SearchUser(
            IUserRepository userRepo,
            IOsuWebClient osuWebClient,
            SocketInteractionContext context, 
            string username)
        {
            User? user;
            if (string.IsNullOrEmpty(username)) {
                user = await userRepo.GetUserWithDiscordId(context.User.Id);
            } else {
                user = await userRepo.GetUserByUsername(username);
                if (user == null) {
                    user = await osuWebClient.FindUserWithUsername(username);
                    if (user != null) await userRepo.AddUser(user);
                }
            }
            return user;
        }

        public static async Task<User?> SearchUser(
            IUserRepository userRepo,
            IOsuWebClient osuWebClient,
            SocketCommandContext context, 
            string username)
        {
            User? user;
            if (string.IsNullOrEmpty(username)) {
                user = await userRepo.GetUserWithDiscordId(context.User.Id);
            } else {
                user = await userRepo.GetUserByUsername(username);
                if (user == null) {
                    user = await osuWebClient.FindUserWithUsername(username);
                    if (user != null) await userRepo.AddUser(user);
                }
            }
            return user;
        }

        public static async Task<User?> SearchUser(
            IUserRepository userRepo,
            IOsuWebClient osuWebClient,
            string username
        )
        {
            User? user = await userRepo.GetUserByUsername(username);
            if (user == null) {
                user = await osuWebClient.FindUserWithUsername(username);
                if (user != null) await userRepo.AddUser(user);
            }
            return user;
        }

        public static async Task<User?> SearchUser(
            IUserRepository userRepo,
            IOsuWebClient osuWebClient,
            ChannelOsuContext context, 
            string username)
        {
            User? user;
            if (string.IsNullOrEmpty(username)) {
                user = await userRepo.GetUserWithDiscordId(context.DiscordUserId);
            } else {
                user = await userRepo.GetUserByUsername(username);
                if (user == null) {
                    user = await osuWebClient.FindUserWithUsername(username);
                    if (user != null) await userRepo.AddUser(user);
                }
            }
            return user;
        }
    }
}
