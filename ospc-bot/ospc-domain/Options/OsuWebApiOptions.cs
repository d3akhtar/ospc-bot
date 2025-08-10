namespace OSPC.Domain.Options
{
	public class OsuWebApiOptions : IAppOptions
	{
		public static string GetOptionName() => "OsuWebApi";
		
		public required int ClientId { get; set; }
		public required string ClientSecret { get; set; }
		public required string BaseUrl { get; set; }
	}
}
