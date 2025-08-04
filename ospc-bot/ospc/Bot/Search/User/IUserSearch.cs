using OSPC.Utils;
using User =  OSPC.Domain.Model.User;

namespace OSPC.Bot.Search.UserSearch
{
	public interface IUserSearch
	{
		public Task<User?> SearchUser(string username, ChannelOsuContext? channelOsuContext = null);
	}
}
