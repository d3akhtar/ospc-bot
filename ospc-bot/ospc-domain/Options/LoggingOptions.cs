namespace OSPC.Domain.Options
{
	public class LoggingOptions : IAppOptions
	{
		public static string GetOptionName() => "Logging";
		
		public required LogLevel LogLevel { get; set; }
		public required bool LogToFile { get; set; }
		public required string LogFilePath { get; set; }
	}

	public enum LogLevel
	{
		Critical,
		Error,
		Warning,
		Info
	}
}
