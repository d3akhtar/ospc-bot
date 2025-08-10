namespace OSPC.Domain.Options
{
	public class CacheOptions
	{
		public static string Name = "Cache";
		
		public required string RedisConnection { get; set; }
		public required int DefaultExpirationMinutes { get; set; }
	}
}
