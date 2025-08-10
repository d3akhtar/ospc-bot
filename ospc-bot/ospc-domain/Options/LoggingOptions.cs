namespace OSPC.Domain.Options
{
	public class LoggingOptions
	{
		public static string Name = "Logging";
		
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
