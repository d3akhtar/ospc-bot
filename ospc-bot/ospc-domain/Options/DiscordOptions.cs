namespace OSPC.Domain.Options
{
    public class DiscordOptions : IAppOptions
    {
        public static string Name { get; } = "Discord";

        public required string Token { get; set; }
    }
}
