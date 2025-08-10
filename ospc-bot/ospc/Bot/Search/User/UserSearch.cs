using OSPC.Domain.Model;
using OSPC.Infrastructure.Database.Repository;
using OSPC.Infrastructure.Http;
using OSPC.Utils;

namespace OSPC.Bot.Search.UserSearch
{
    public class UserSearch : IUserSearch
    {
        private readonly IUserRepository _userRepo;
        private readonly IOsuWebClient _osuWebClient;

        public UserSearch(IUserRepository userRepo, IOsuWebClient osuWebClient)
		{
			_userRepo = userRepo;
			_osuWebClient = osuWebClient;
		}
		
        public async Task<User?> SearchUser(string username, ChannelOsuContext? channelOsuContext = null)
        {
			List<Task<User?>> userSearchTasks = new();

			if (channelOsuContext != null && username == User.Unspecified)
				userSearchTasks.Add(_userRepo.GetUserWithDiscordId(channelOsuContext.DiscordUserId));
			else
				userSearchTasks.Add(_userRepo.GetUserByUsername(username));

			userSearchTasks.Add(_osuWebClient.FindUserWithUsername(username));

			foreach (var task in userSearchTasks) {
				var user = await task;
				if (user != null) {
					await _userRepo.AddUser(user);
					return user;
				}
			}

			return null;
        }
    }
}
