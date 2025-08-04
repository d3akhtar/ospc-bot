namespace OSPC.Domain.Model
{
    public class DiscordPlayer
    {
        public ulong DiscordUserId { get; set; }
        public int PlayerUserId { get; set; }
        public string PlayerUsername { get; set; } = "[unnamed]";
    }
}
