using Microsoft.Extensions.Logging;

using OSPC.Domain.Model;
using OSPC.Infrastructure.Database.Repository;
using OSPC.Infrastructure.Http;
using OSPC.Utils;

namespace OSPC.Bot.Search.UserSearch
{
    public class UserSearch : IUserSearch
    {
        private readonly ILogger<UserSearch> _logger;
        private readonly IUserRepository _userRepo;
        private readonly IOsuWebClient _osuWebClient;

        public UserSearch(ILogger<UserSearch> logger, IUserRepository userRepo, IOsuWebClient osuWebClient)
        {
            _logger = logger;
            _userRepo = userRepo;
            _osuWebClient = osuWebClient;
        }

        public async Task<Result<User>> SearchUser(string username, ChannelOsuContext? channelOsuContext = null)
        {
            _logger.LogDebug("Searching for user using username: {Username} and context: {@ChannelOsuContext}", username, channelOsuContext);

            var userResult = channelOsuContext != null && username == User.Unspecified ?
                await _userRepo.GetUserWithDiscordId(channelOsuContext.DiscordUserId) :
                await _userRepo.GetUserByUsername(username);

            if (!userResult.Successful)
            {
                var user = await _osuWebClient.FindUserWithUsername(username);
                if (user is { } u)
                {
                    await _userRepo.AddUser(u);
                    return user;
                }
            }

            return userResult;
        }
    }
}