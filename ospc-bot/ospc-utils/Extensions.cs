using Discord.Commands;
using Discord.Interactions;

namespace OSPC.Utils
{
    public static class Extensions
    {
        public static ChannelOsuContext GetOsuContext(this SocketInteractionContext socketCtx)
            => new ChannelOsuContext {
                ChannelId = socketCtx.Channel.Id,
                DiscordUserId = socketCtx.User.Id,
            };
        
        public static ChannelOsuContext GetOsuContext(this SocketCommandContext socketCtx)
            => new ChannelOsuContext {
                ChannelId = socketCtx.Channel.Id,
                DiscordUserId = socketCtx.User.Id
            };
    }
}
