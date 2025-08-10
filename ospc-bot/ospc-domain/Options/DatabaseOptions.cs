namespace OSPC.Domain.Options
{
	public class DatabaseOptions
	{
		public static string Name = "Database";
		
		public required string ConnectionString { get; set; }
	}
}
