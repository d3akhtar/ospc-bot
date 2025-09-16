namespace OSPC.Utils
{
    public class ChannelOsuContext
    {
        public ulong DiscordUserId { get; set; }
        public ulong ChannelId { get; set; }

        public static ChannelOsuContext Empty => new ChannelOsuContext
        {
            DiscordUserId = 0,
            ChannelId = 0
        };
    }
}