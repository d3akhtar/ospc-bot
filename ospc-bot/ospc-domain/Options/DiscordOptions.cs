namespace OSPC.Domain.Options
{
	public class DiscordOptions : IAppOptions
	{
		public static string GetOptionName() => "Discord";
		
		public required string Token { get; set; }
	}
}
