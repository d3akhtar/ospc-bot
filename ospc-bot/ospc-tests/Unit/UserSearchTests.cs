using Moq;
using OSPC.Bot.Search.UserSearch;
using OSPC.Infrastructure.Database.Repository;
using OSPC.Domain.Model;
using Microsoft.Extensions.Logging;
using OSPC.Infrastructure.Http;
using OSPC.Utils;

namespace OSPC.Tests.Unit
{
	public class UserSearchTests
	{
		private readonly Mock<IUserRepository> _userRepoMock;
		private readonly Mock<IOsuWebClient> _osuWebClientMock;
		private readonly UserSearch _userSearch;

		public UserSearchTests()
		{			
			var mockUser = new User { Id = 1, Username = "opensand", CountryCode = "CA" };

			_userRepoMock = new();
			_userRepoMock.Setup(u => u.GetUserByUsername(It.IsAny<string>())).ReturnsAsync(mockUser);
			_userRepoMock.Setup(u => u.GetUserWithDiscordId(It.IsAny<ulong>())).ReturnsAsync(mockUser);

			_osuWebClientMock = new();
			_osuWebClientMock.Setup(o => o.FindUserWithUsername(It.IsAny<string>())).ReturnsAsync(mockUser);
			
			_userSearch = new(Mock.Of<ILogger<UserSearch>>(), _userRepoMock.Object, _osuWebClientMock.Object);
		}
		
		[Fact]
		public async Task SpecifiedUserTest()
		{
			var result = await _userSearch.SearchUser("opensand");

			_userRepoMock.Verify(u => u.GetUserByUsername(It.IsAny<string>()), Times.Exactly(1));
			_userRepoMock.Verify(u => u.GetUserWithDiscordId(It.IsAny<ulong>()), Times.Never);
			_osuWebClientMock.Verify(o => o.FindUserWithUsername(It.IsAny<string>()), Times.Never);
			Assert.NotNull(result);
		}

		[Fact]
		public async Task SearchUsingChannelContextTest()
		{
			var result = await _userSearch.SearchUser(User.Unspecified, new ChannelOsuContext { DiscordUserId = 1, ChannelId = 1 });

			_userRepoMock.Verify(u => u.GetUserByUsername(It.IsAny<string>()), Times.Never);
			_userRepoMock.Verify(u => u.GetUserWithDiscordId(It.IsAny<ulong>()), Times.Exactly(1));
			_osuWebClientMock.Verify(o => o.FindUserWithUsername(It.IsAny<string>()), Times.Never);
			Assert.NotNull(result);
		}
		
		[Fact]
		public async Task SearchUsingOsuWebClient1()
		{
			_userRepoMock.Setup(u => u.GetUserByUsername(It.IsAny<string>())).ReturnsAsync((User)null!);
			
			var result = await _userSearch.SearchUser("opensand");

			_userRepoMock.Verify(u => u.GetUserByUsername(It.IsAny<string>()), Times.Exactly(1));
			_userRepoMock.Verify(u => u.GetUserWithDiscordId(It.IsAny<ulong>()), Times.Never);
			_osuWebClientMock.Verify(o => o.FindUserWithUsername(It.IsAny<string>()), Times.Exactly(1));
			_userRepoMock.Verify(u => u.AddUser(It.IsAny<User>()), Times.Exactly(1));
			Assert.NotNull(result);
		}

		[Fact]
		public async Task SearchUsingOsuWebClient2()
		{
			_userRepoMock.Setup(u => u.GetUserWithDiscordId(It.IsAny<ulong>())).ReturnsAsync((User)null!);
			
			var result = await _userSearch.SearchUser(User.Unspecified, new ChannelOsuContext { DiscordUserId = 1, ChannelId = 1 });

			_userRepoMock.Verify(u => u.GetUserByUsername(It.IsAny<string>()), Times.Never);
			_userRepoMock.Verify(u => u.GetUserWithDiscordId(It.IsAny<ulong>()), Times.Exactly(1));
			_osuWebClientMock.Verify(o => o.FindUserWithUsername(It.IsAny<string>()), Times.Exactly(1));
			_userRepoMock.Verify(u => u.AddUser(It.IsAny<User>()), Times.Exactly(1));
			Assert.NotNull(result);
		}

		[Fact]
		public async Task SearchUsingOsuWebClient3()
		{
			_userRepoMock.Setup(u => u.GetUserByUsername(It.IsAny<string>())).ReturnsAsync((User)null!);
			_osuWebClientMock.Setup(o => o.FindUserWithUsername(It.IsAny<string>())).ReturnsAsync((User)null!);
			
			var result = await _userSearch.SearchUser(User.Unspecified);

			_userRepoMock.Verify(u => u.GetUserByUsername(It.IsAny<string>()), Times.Exactly(1));
			_userRepoMock.Verify(u => u.GetUserWithDiscordId(It.IsAny<ulong>()), Times.Never);
			_osuWebClientMock.Verify(o => o.FindUserWithUsername(It.IsAny<string>()), Times.Exactly(1));
			_userRepoMock.Verify(u => u.AddUser(It.IsAny<User>()), Times.Never);
			Assert.Null(result);
		}
	}
}
