using Discord.Commands;
using Discord.Interactions;

using OSPC.Bot.Context;

namespace OSPC.Bot.Extensions
{
    public static class DiscordSocketExtensions
    {
        public static ChannelOsuContext GetOsuContext(this SocketInteractionContext? socketCtx)
            => socketCtx is not null ?
                new ChannelOsuContext
                {
                    ChannelId = socketCtx.Channel.Id,
                    DiscordUserId = socketCtx.User.Id,
                } : ChannelOsuContext.Empty;

        public static ChannelOsuContext GetOsuContext(this SocketCommandContext? socketCtx)
            => socketCtx is not null ?
                new ChannelOsuContext
                {
                    ChannelId = socketCtx.Channel.Id,
                    DiscordUserId = socketCtx.User.Id,
                } : ChannelOsuContext.Empty;
    }    
}
