using Microsoft.Extensions.Logging;
using Moq;
using OSPC.Tests.Extensions;
using OSPC.Bot.Command;
using OSPC.Infrastructure.Http;
using OSPC.Infrastructure.Database.Repository;
using OSPC.Bot.Search.UserSearch;
using OSPC.Infrastructure.Job;
using ChannelOsuContext = OSPC.Utils.ChannelOsuContext;
using SearchParams = OSPC.Utils.Parsing.SearchParams;
using Xunit.Abstractions;
using OSPC.Domain.Model;
using OSPC.Utils;

namespace OSPC.Tests.Unit
{
	public class BotCommandServiceTests
	{
		private readonly ITestOutputHelper _output;
		
		private BotCommandService _botCmds;

		private Mock<ILogger<BotCommandService>> _loggerMock;
		private Mock<IUserRepository> _userRepoMock;
		private Mock<IBeatmapRepository> _beatmapRepoMock;
		private Mock<IUserSearch> _userSearchMock;
		private Mock<IOsuWebClient> _osuWebClientMock;
		private Mock<IPlaycountFetchJobQueue> _playcountFetchJobQueueMock;

		public static IEnumerable<object[]> SearchParamTestData =
		[
			[ SearchParams.ForMostPlayed("opensand") ],
			[ SearchParams.Parse("opensand c>10 c<20 cs>=2").Value! ]
		];
		
		public BotCommandServiceTests(ITestOutputHelper output)
		{
			_output = output;
			SetupMocks();
			_botCmds = new (_loggerMock!.Object, _userRepoMock!.Object, _beatmapRepoMock!.Object, _userSearchMock!.Object, _osuWebClientMock!.Object, _playcountFetchJobQueueMock!.Object);
		}

		private void SetupMocks()
		{			
			var mockUser = new User { Id = 1, Username = "MockUser", CountryCode = "Mock country code" };
			var mockBeatmap = new Beatmap { Id = 727, Version = "Mock version" };
			var mockBeatmapPlaycount = new BeatmapPlaycount
			{
				BeatmapId = mockBeatmap.Id,
				UserId = mockUser.Id,
				Count = 100,
				Beatmap = mockBeatmap,
				BeatmapSet = new()
				{
					Artist = "Mock artist",
					Title = "Mock title",
					Covers = new()
					{
						SlimCover2x = "https://mockUrlForSlimCover.com"
					}
				}
			};
			
			_loggerMock = new();
			
			_userRepoMock = new();
			_userRepoMock
				.Setup(u => u.GetUserById(It.IsAny<int>()))
				.ReturnsAsync(mockUser);
				
			_beatmapRepoMock = new();

			_beatmapRepoMock
				.Setup(b => b.GetReferencedBeatmapIdForChannel(It.IsAny<ulong>()))
				.ReturnsAsync(mockBeatmap.Id);
			_beatmapRepoMock
				.Setup(b => b.GetBeatmapPlaycountForUserOnMap(It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(mockBeatmapPlaycount);
			_beatmapRepoMock
				.Setup(b => b.FilterBeatmapPlaycountsForUser(It.IsAny<SearchParams>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(new List<BeatmapPlaycount>() { mockBeatmapPlaycount });
			_beatmapRepoMock
				.Setup(b => b.GetTotalResultCountForSearch(It.IsAny<SearchParams>(), It.IsAny<int>()))
				.ReturnsAsync(100);

			_userSearchMock = new();
			_userSearchMock
				.Setup(u => u.SearchUser(It.IsAny<string>(), It.IsAny<ChannelOsuContext>()))
				.ReturnsAsync(mockUser);

			_osuWebClientMock = new();
			_osuWebClientMock
				.Setup(o => o.GetUserRankStatistics(It.IsAny<int>()))
				.ReturnsAsync(new UserRankStatistic { Pp = 1, Rank = 100000, CountryRank = 1000});

			_playcountFetchJobQueueMock = new();
		}

		
		[Theory]
		[InlineData("https://osu.ppy.sh/beatmapsets/399151#osu/965524")]
		[InlineData("https://osu.ppy.sh/beatmapsets/650738#osu/1378892")]
		[InlineData("https://osu.ppy.sh/beatmapsets/891581#osu/1863868")]
		[InlineData("https://osu.ppy.sh/beatmapsets/123#osu/123456")]
		public async Task GetPlaycountFromBeatmapLinkSuccessTest(string beatmapLink)
		{
			var result = await _botCmds.GetPlaycount(ChannelOsuContext.Empty, "opensand", beatmapLink);

			_output.WriteLine($"Result embed description: {result.Embed.Description}");

			_loggerMock.Verify(LogLevel.Information, Times.Exactly(1));

			_userSearchMock.Verify(u => u.SearchUser(It.IsAny<string>(), It.IsAny<ChannelOsuContext>()), Times.Exactly(1));
			_beatmapRepoMock.Verify(b => b.GetBeatmapPlaycountForUserOnMap(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
			_osuWebClientMock.Verify(o => o.GetUserRankStatistics(It.IsAny<int>()), Times.Exactly(1));

			Assert.True(result.Successful);
		}
		
		[Theory]
		[InlineData("[not properly formatted osu beatmap link]")]
		[InlineData("https://osu.ppy.sh/beatmapsets/891581")]
		[InlineData("https://osu.ppy.sh/beatmapsets/2383590#mania/5154388")]
		public async Task GetPlaycountFromBeatmapLinkInvalidLinkTest(string invalidBeatmapLink)
		{
			var result = await _botCmds.GetPlaycount(ChannelOsuContext.Empty, "opensand", invalidBeatmapLink);

			_output.WriteLine($"Result embed description: {result.Embed.Description}");

			_loggerMock.Verify(LogLevel.Information, Times.Never());

			Assert.False(result.Successful);
			Assert.Equivalent(result.Embed.Description, "Invalid beatmap link format");
		}

		[Theory, MemberData(nameof(SearchParamTestData))]
		public async Task SearchTest(SearchParams searchParams)
		{
			var result = await _botCmds.Search(ChannelOsuContext.Empty, searchParams);

			_output.WriteLine($"Result embed description: {result.Embed.Description}");			

			_loggerMock.Verify(LogLevel.Information, Times.Exactly(1));

			_userSearchMock.Verify(u => u.SearchUser(It.IsAny<string>(), It.IsAny<ChannelOsuContext>()), Times.Exactly(1));
			_beatmapRepoMock.Verify(b => b.FilterBeatmapPlaycountsForUser(It.IsAny<SearchParams>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));

			Assert.True(result.Successful);
		}

		[Fact]
		public async Task LinkProfileSuccessTest()
		{
			string username = "opensand";
			
			_userRepoMock
				.Setup(u => u.AddDiscordPlayerMapping(It.IsAny<ulong>(), It.IsAny<int>()))
				.ReturnsAsync(true);
				
			var result = await _botCmds.LinkProfile(ChannelOsuContext.Empty, username);

			_output.WriteLine($"Result embed description: {result.Embed.Description}");
			
			_loggerMock.Verify(LogLevel.Information, Times.Exactly(1));

			_userSearchMock.Verify(u => u.SearchUser(It.IsAny<string>(), It.IsAny<ChannelOsuContext>()), Times.Exactly(1));
			_userRepoMock.Verify(u => u.AddDiscordPlayerMapping(It.IsAny<ulong>(), It.IsAny<int>()), Times.Exactly(1));

			Assert.True(result.Successful);
		}

		[Fact]
		public async Task LinkProfileFailTest1()
		{
			string username = "opensand";
			
			_userRepoMock
				.Setup(u => u.AddDiscordPlayerMapping(It.IsAny<ulong>(), It.IsAny<int>()))
				.ReturnsAsync(false);
				
			var result = await _botCmds.LinkProfile(ChannelOsuContext.Empty, username);

			_output.WriteLine($"Result embed description: {result.Embed.Description}");

			_loggerMock.Verify(LogLevel.Information, Times.Exactly(1));

			_userSearchMock.Verify(u => u.SearchUser(It.IsAny<string>(), It.IsAny<ChannelOsuContext>()), Times.Exactly(1));
			_userRepoMock.Verify(u => u.AddDiscordPlayerMapping(It.IsAny<ulong>(), It.IsAny<int>()), Times.Exactly(1));

			Assert.False(result.Successful);
			Assert.Equivalent($"Something went wrong while mapping user {username}", result.Embed.Description);
		}
	}
}
