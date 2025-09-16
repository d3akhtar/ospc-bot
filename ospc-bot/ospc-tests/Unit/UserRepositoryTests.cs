using Microsoft.Extensions.Logging;

using Moq;

using MySql.Data.MySqlClient;

using OSPC.Domain.Model;
using OSPC.Infrastructure.Database;
using OSPC.Infrastructure.Database.CommandFactory;
using OSPC.Infrastructure.Database.Repository;
using OSPC.Tests.DefaultValueProviders;
using OSPC.Tests.Extensions;
using OSPC.Tests.Mocks;

namespace OSPC.Tests.Unit
{
    public class UserRepositoryTests
    {
        private readonly UserRepository _userRepo;

        private readonly Mock<ILogger<UserRepository>> _loggerMock;
        private readonly MockDatabase _dbMock;
        private readonly Mock<ICommandFactory> _commandFactoryMock;

        public UserRepositoryTests()
        {
            _loggerMock = new();
            _commandFactoryMock = new(MockBehavior.Loose) { DefaultValueProvider = new DatabaseDefaultValueProvider() };
            _dbMock = new();

            _userRepo = new(_loggerMock.Object, _dbMock, _commandFactoryMock.Object);
        }

        [Fact]
        public async Task AddDiscordPlayerMappingTest()
        {
            await _userRepo.AddDiscordPlayerMapping(1, 1);
            _loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));
            _commandFactoryMock.Verify(cf => cf.CreateAddDiscordPlayerMappingCommand(It.IsAny<MySqlConnection>(), It.IsAny<ulong>(), It.IsAny<int>()), Times.Exactly(1));
        }

        [Fact]
        public async Task GetPlayerInfoFromDiscordIdTest()
        {
            await _userRepo.GetPlayerInfoFromDiscordId(1);
            _loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));
            _commandFactoryMock.Verify(cf => cf.CreateGetPlayerInfoFromDiscordIdCommand(It.IsAny<MySqlConnection>(), It.IsAny<ulong>()), Times.Exactly(1));
        }

        [Fact]
        public async Task AddUserTest()
        {
            await _userRepo.AddUser(new User());
            _loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));
            _commandFactoryMock.Verify(cf => cf.CreateAddUserCommand(It.IsAny<MySqlConnection>(), It.IsAny<User>()), Times.Exactly(1));
        }

        [Fact]
        public async Task GetUserByIdTest()
        {
            await _userRepo.GetUserById(1);
            _loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));
            _commandFactoryMock.Verify(cf => cf.CreateGetUserByIdCommand(It.IsAny<MySqlConnection>(), It.IsAny<int>()), Times.Exactly(1));
        }

        [Fact]
        public async Task GetUserByUsernameTest()
        {
            await _userRepo.GetUserByUsername("opensand");
            _loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));
            _commandFactoryMock.Verify(cf => cf.CreateGetUserByUsernameCommand(It.IsAny<MySqlConnection>(), It.IsAny<string>()), Times.Exactly(1));
        }

        [Fact]
        public async Task GetUserWithDiscordIdTest()
        {
            await _userRepo.GetUserWithDiscordId(1);
            _loggerMock.Verify(LogLevel.Debug, Times.Exactly(1));
            _commandFactoryMock.Verify(cf => cf.CreateGetUserWithDiscordIdCommand(It.IsAny<MySqlConnection>(), It.IsAny<ulong>()), Times.Exactly(1));
        }
    }
}