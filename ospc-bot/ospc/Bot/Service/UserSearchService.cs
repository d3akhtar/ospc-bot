using Microsoft.Extensions.Logging;
using OSPC.Bot.Context;
using OSPC.Domain.Common;
using OSPC.Domain.Constants;
using OSPC.Domain.Model;
using OSPC.Infrastructure.Database.Repository;
using OSPC.Infrastructure.Http;

namespace OSPC.Bot.Service
{
    public class UserSearchService : IUserSearchService
    {
        private readonly ILogger<UserSearchService> _logger;
        private readonly IUserRepository _userRepo;
        private readonly IOsuWebClient _osuWebClient;

        public UserSearchService(ILogger<UserSearchService> logger, IUserRepository userRepo, IOsuWebClient osuWebClient)
        {
            _logger = logger;
            _userRepo = userRepo;
            _osuWebClient = osuWebClient;
        }

        public async Task<Result<User>> SearchUser(string username, ChannelOsuContext? channelOsuContext = null)
        {
            _logger.LogDebug("Searching for user using username: {Username} and context: {@ChannelOsuContext}", username, channelOsuContext);

            var userResult = channelOsuContext is not null && username is Unspecified.User ?
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
