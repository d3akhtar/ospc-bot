using Discord.Commands;
using Discord.Interactions;
using OSPC.Domain.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OSPC.Utils
{
    public static class DiscordSocketExtensions
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
