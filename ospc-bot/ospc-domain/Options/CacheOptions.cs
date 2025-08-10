namespace OSPC.Domain.Options
{
	public class CacheOptions : IAppOptions
	{
        public static string GetOptionName() => "Cache";
		
		public required string RedisConnection { get; set; }
		public required int DefaultExpirationMinutes { get; set; }
    }
}
