using Discord.Commands;
using Discord.Interactions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OSPC.Domain.Options;

namespace OSPC.Utils
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

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppOption<T>(this IServiceCollection serviceCollection, IConfiguration config) where T : class, IAppOptions
        {
            serviceCollection
                .AddOptions<T>()
                .Bind(config.GetSection(T.GetOptionName()))
                .ValidateDataAnnotations();

            return serviceCollection;
        }
    }
}