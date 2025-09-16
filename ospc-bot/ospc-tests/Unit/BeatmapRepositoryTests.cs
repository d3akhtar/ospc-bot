using OSPC.Infrastructure.Database;
using OSPC.Infrastructure.Database.CommandFactory;
using OSPC.Infrastructure.Database.TransactionFactory;
using OSPC.Infrastructure.Database.Repository;
using Moq;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using OSPC.Tests.Mocks;
using OSPC.Domain.Model;
using OSPC.Tests.Extensions;
using OSPC.Utils.Parsing;
using OSPC.Utils;
using OSPC.Tests.DefaultValueProviders;

namespace OSPC.Tests.Unit
{
	public class BeatmapRepositoryTests
	{
		private BeatmapRepository _beatmapRepo;

		private Mock<ILogger<BeatmapRepository>> _loggerMock;
		private MockDatabase _dbMock;
		private Mock<ICommandFactory> _commandFactoryMock;
		private Mock<ITransactionFactory> _transactionFactoryMock;

		public BeatmapRepositoryTests()
		{
			_loggerMock = new();
			_commandFactoryMock = new(MockBehavior.Loose) { DefaultValueProvider = new DatabaseDefaultValueProvider() };
			_transactionFactoryMock = new(MockBehavior.Loose) { DefaultValueProvider = new DatabaseDefaultValueProvider() };
			_dbMock = new();

			_beatmapRepo = new(_loggerMock.Object, _dbMock, _commandFactoryMock.Object, _transactionFactoryMock.Object);
		}

		[Fact]
		public async Task AddBeatmapPlaycountsAsyncTest()
		{
			await _beatmapRepo.AddBeatmapPlaycountsAsync([]);
			Assert.Empty(_commandFactoryMock.Invocations);
			_loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));
			_transactionFactoryMock.Verify(tf => tf.CreateAddBeatmapPlaycountTransaction(It.IsAny<MySqlConnection>(), It.IsAny<IEnumerable<BeatmapPlaycount>>()), Times.Exactly(1));
		}

		[Fact]
		public async Task AddBeatmapsAsyncTest()
		{
			await _beatmapRepo.AddBeatmapsAsync([]);
			Assert.Empty(_commandFactoryMock.Invocations);
			_loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));
			_transactionFactoryMock.Verify(tf => tf.CreateAddBeatmapTransaction(It.IsAny<MySqlConnection>(), It.IsAny<IEnumerable<Beatmap>>()), Times.Exactly(1));			
		}

		[Fact]
		public async Task AddBeatmapSetsAsyncTest()
		{
			await _beatmapRepo.AddBeatmapSetsAsync([]);
			Assert.Empty(_commandFactoryMock.Invocations);
			_loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));
			_transactionFactoryMock.Verify(tf => tf.CreateAddBeatmapSetTransaction(It.IsAny<MySqlConnection>(), It.IsAny<IEnumerable<BeatmapSet>>()), Times.Exactly(1));			
		}

		[Fact]
		public async Task FilterBeatmapPlaycountsForUserTest()
		{
			await _beatmapRepo.FilterBeatmapPlaycountsForUser(SearchParams.ForMostPlayed("opensand"), 1, 1, 1);
			Assert.Empty(_transactionFactoryMock.Invocations);
			_loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));
			_commandFactoryMock.Verify(cf => cf.CreateBeatmapPlaycountFilterCommand(It.IsAny<MySqlConnection>(), It.IsAny<SearchParams>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
		}

		[Fact]
		public async Task GetBeatmapByIdTest()
		{
			await _beatmapRepo.GetBeatmapById(1);
			Assert.Empty(_transactionFactoryMock.Invocations);
			_loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));
			_commandFactoryMock.Verify(cf => cf.CreateGetBeatmapByIdCommand(It.IsAny<MySqlConnection>(), It.IsAny<int>()), Times.Exactly(1));
		}

		[Fact]
		public async Task GetBeatmapPlaycountsForUserTest()
		{
			await _beatmapRepo.GetBeatmapPlaycountsForUser(1, 1, 1);
			Assert.Empty(_transactionFactoryMock.Invocations);
			_loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));
			_commandFactoryMock.Verify(cf => cf.CreateBeatmapPlaycountFilterCommand(It.IsAny<MySqlConnection>(), It.IsAny<SearchParams>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
		}

		[Fact]
		public async Task GetBeatmapPlaycountForUserOnMapTest()
		{			
			await _beatmapRepo.GetBeatmapPlaycountForUserOnMap(1, 1);
			Assert.Empty(_transactionFactoryMock.Invocations);
			_loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));
			_commandFactoryMock.Verify(cf => cf.CreateGetBeatmapPlaycountForUserCommand(It.IsAny<MySqlConnection>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
		}

		[Fact]
		public async Task UpdateReferencedBeatmapIdForChannelTest()
		{			
			await _beatmapRepo.UpdateReferencedBeatmapIdForChannel(1, 1);
			Assert.Empty(_transactionFactoryMock.Invocations);
			_loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));
			_commandFactoryMock.Verify(cf => cf.CreateUpdateReferencedBeatmapIdForChannelCommand(It.IsAny<MySqlConnection>(), It.IsAny<ulong>(), It.IsAny<int>()), Times.Exactly(1));
		}

		[Fact]
		public async Task GetReferencedBeatmapIdForChannelTest()
		{			
			await _beatmapRepo.GetReferencedBeatmapIdForChannel(1);
			Assert.Empty(_transactionFactoryMock.Invocations);
			_loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));
			_commandFactoryMock.Verify(cf => cf.CreateGetReferencedBeatmapIdForChannelCommand(It.IsAny<MySqlConnection>(), It.IsAny<ulong>()), Times.Exactly(1));
		}

		[Fact]
		public async Task GetTotalResultCountForSearchTest()
		{			
			await _beatmapRepo.GetTotalResultCountForSearch(SearchParams.ForMostPlayed("opensand"), 1);
			Assert.Empty(_transactionFactoryMock.Invocations);
			_loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));
			_commandFactoryMock.Verify(cf => cf.CreateBeatmapPlaycountFilterCommand(It.IsAny<MySqlConnection>(), It.IsAny<SearchParams>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
		}
	}
}
