using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using OSPC.Infrastructure.Caching;
using OSPC.Tests.Extensions;

using StackExchange.Redis;

using Xunit.Abstractions;

using CacheOptions = OSPC.Domain.Options.CacheOptions;
using UserRankStatistic = OSPC.Domain.Model.UserRankStatistic;

namespace OSPC.Tests.Unit
{
    public class RedisServiceTests
    {
        private readonly ITestOutputHelper _output;

        private readonly RedisService _redisService;

        private readonly Mock<ILogger<RedisService>> _loggerMock;
        private readonly Mock<IOptions<CacheOptions>> _cacheOptionsMock;
        private readonly Mock<IConnectionMultiplexer> _connectionMock;
        private readonly Mock<IDatabase> _cacheMock;

        public RedisServiceTests(ITestOutputHelper output)
        {
            _output = output;

            _loggerMock = new();
            _cacheOptionsMock = new();
            _connectionMock = new();
            _cacheMock = new();

            _connectionMock.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object?>())).Returns(_cacheMock.Object);

            _redisService = new(_loggerMock.Object, _cacheOptionsMock.Object, _connectionMock.Object);
        }

        [Fact]
        public async Task GetUncachedAccessTokenAsyncTest()
        {
            var accessToken = await _redisService.GetAccessTokenAsync();

            _loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));
            _loggerMock.Verify(LogLevel.Warning, Times.Exactly(1));

            Assert.Null(accessToken);
        }

        [Fact]
        public async Task GetCachedAccessTokenAsyncTest()
        {
            _cacheMock.Setup(c => c.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>())).ReturnsAsync("access token");

            var accessToken = await _redisService.GetAccessTokenAsync();

            _loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));

            Assert.NotNull(accessToken);
        }

        [Fact]
        public async Task SetAccessTokenAsyncTest()
        {
            await _redisService.SetAccessTokenAsync("value", 10);

            _loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));

            _cacheMock.Verify(c => c.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.AtLeastOnce());
        }

        [Fact]
        public async Task GetUncachedQueryAsyncTest()
        {
            var value = await _redisService.GetQuery<string>("key");

            _loggerMock.Verify(LogLevel.Debug, Times.Exactly(2));

            _cacheMock.Verify(c => c.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.AtLeastOnce());

            Assert.Null(value);
        }

        [Fact]
        public async Task GetCachedQueryAsyncTest()
        {
            _cacheMock.Setup(c => c.StringGetAsync("validKey", It.IsAny<CommandFlags>())).ReturnsAsync("{ \"pp\": 10, \"global_rank\": 10, \"country_rank\": 10 }");

            var value = await _redisService.GetQuery<UserRankStatistic>("validKey");

            Assert.IsType<UserRankStatistic>(value);

            _loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));

            Assert.NotNull(value);

            value = await _redisService.GetQuery<UserRankStatistic>("invalidKey");
            Assert.Null(value);
        }

        [Fact]
        public async Task InvalidateKeysTest()
        {
            var numberOfKeys = new Random().Next(1, 100);

            _output.WriteLine($"Generating {numberOfKeys} random keys");

            List<string> keysToInvalidate = new();
            for (int i = 0; i < numberOfKeys; i++)
                keysToInvalidate.Add(Guid.NewGuid().ToString());

            await _redisService.InvalidateKeys(keysToInvalidate);

            _loggerMock.Verify(LogLevel.Debug, Times.Exactly(numberOfKeys));
        }

        [Fact]
        public async Task SetQueryAsyncTest()
        {
            await _redisService.SaveQuery<UserRankStatistic>("key", new UserRankStatistic { Pp = 10, CountryRank = 10, Rank = 10 });

            _loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));

            _cacheMock.Verify(c => c.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.AtLeastOnce());
        }

        [Fact]
        public async Task GetUncachedUserRankStatisticAsyncTest()
        {
            var value = await _redisService.GetUserRankStatisticAsync(new Random().Next());

            _loggerMock.Verify(LogLevel.Debug, Times.Exactly(2));

            _cacheMock.Verify(c => c.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.AtLeastOnce());

            Assert.Null(value);
        }

        [Fact]
        public async Task GetCachedUserRankStatisticAsyncTest()
        {
            _cacheMock.Setup(c => c.StringGetAsync("user_stat_1", It.IsAny<CommandFlags>())).ReturnsAsync("{ \"pp\": 10, \"global_rank\": 10, \"country_rank\": 10 }");

            var value = await _redisService.GetUserRankStatisticAsync(1);

            Assert.IsType<UserRankStatistic>(value);

            _loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));

            Assert.NotNull(value);

            value = await _redisService.GetUserRankStatisticAsync(new Random().Next(2, int.MaxValue));
            Assert.Null(value);
        }
    }
}