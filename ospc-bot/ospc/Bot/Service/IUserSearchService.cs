using OSPC.Bot.Context;
using OSPC.Domain.Common;

using User = OSPC.Domain.Model.User;

namespace OSPC.Bot.Service
{
    public interface IUserSearchService
    {
        public Task<Result<User>> SearchUser(string username, ChannelOsuContext? channelOsuContext = null);
    }
}
