using Moq;
using Microsoft.Extensions.Logging;

namespace OSPC.Tests.Extensions
{
	public static class LoggerMockExtensions
	{
		public static void Verify<T>(this Mock<ILogger<T>> logger, LogLevel logLevel, Times times)
		{
			logger.Verify(l => l.Log(
				logLevel,
				It.IsAny<EventId>(),
				It.IsAny<It.IsAnyType>(),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
				times
			);
		}
	}
}
