using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OSPC.Bot.Extensions;
using OSPC.Bot.Service;
using OSPC.Domain.Options;
using OSPC.Infrastructure.Caching;
using OSPC.Infrastructure.Database;
using OSPC.Infrastructure.Database.CommandFactory;
using OSPC.Infrastructure.Database.Repository;
using OSPC.Infrastructure.Database.TransactionFactory;
using OSPC.Infrastructure.Http;
using OSPC.Infrastructure.Job;
using Serilog;
using StackExchange.Redis;

using IDatabase = OSPC.Infrastructure.Database.IDatabase;

namespace OSPC.Bot
{
    internal static class DependencyInjection
    {
        public static IServiceCollection GetServiceCollection(CommandLineArgs args)
        {        
            var config = new ConfigurationBuilder()
                .AddJsonFile(args.AppsettingsFilePath, optional: false, reloadOnChange: true)
                .Build();
                
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();
                
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(builder =>
            {
                builder.AddSerilog();
            });

            serviceCollection
                .AddAppOption<DatabaseOptions>(config)
                .AddAppOption<DiscordOptions>(config)
                .AddAppOption<OsuWebApiOptions>(config)
                .AddAppOption<CacheOptions>(config)
                .AddScoped<IConnectionMultiplexer, ConnectionMultiplexer>(sp =>
                {
                    var cacheOptions = sp.GetRequiredService<IOptions<CacheOptions>>();
                    return ConnectionMultiplexer.Connect(cacheOptions.Value.RedisConnection);
                })
                .AddSingleton<IOsuWebClient, OsuWebClient>()
                .AddSingleton<IRedisService>(sp =>
                {
                    return args.DisableCaching ?
                        new DisabledRedisService():
                        new RedisService(sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RedisService>>(), sp.GetRequiredService<IOptions<CacheOptions>>(), sp.GetRequiredService<IConnectionMultiplexer>());     
                })
                .AddSingleton<IPlaycountFetchJobQueue, PlaycountFetchJobQueue>()
                .AddScoped<IDatabase, MySqlDatabase>()
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IBeatmapRepository, BeatmapRepository>()
                .AddScoped<IOsuWebClient, OsuWebClient>()
                .AddScoped<IUserSearchService, UserSearchService>()
                .AddScoped<IBotCommandService, BotCommandService>()
                .AddSingleton<ICommandFactory, CommandFactory>()
                .AddSingleton<ITransactionFactory, TransactionFactory>();

            return serviceCollection;
        }
    }
}
