namespace OSPC.Domain.Options
{
	public class OsuWebApiOptions
	{
		public static string Name = "OsuWebApi";
		
		public required int ClientId { get; set; }
		public required string ClientSecret { get; set; }
		public required string BaseUrl { get; set; }
	}
}
