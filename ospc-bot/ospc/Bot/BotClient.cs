using System.Reflection;
using System.Text.Json;

using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using OSPC.Bot.Command;
using OSPC.Bot.Component;
using OSPC.Bot.Logging;
using OSPC.Bot.MessageHandlers;
using OSPC.Bot.Search.UserSearch;
using OSPC.Domain.Options;
using OSPC.Infrastructure.Caching;
using OSPC.Infrastructure.Database;
using OSPC.Infrastructure.Database.CommandFactory;
using OSPC.Infrastructure.Database.Repository;
using OSPC.Infrastructure.Database.TransactionFactory;
using OSPC.Infrastructure.Http;
using OSPC.Infrastructure.Job;
using OSPC.Utils;

using Serilog;

using StackExchange.Redis;

using IDatabase = OSPC.Infrastructure.Database.IDatabase;

namespace OSPC.Bot
{
    public class BotClient
    {
        public static BotClient Instance { get; private set; }

        private readonly CommandLineArgs _args;

        private DiscordSocketClient _client;
        private CommandService _cmds;
        private InteractionService _interactService;
        private IServiceProvider _serviceProvider;
        private IConfigurationRoot _config;

        private SimpleMessageHandler _simpleMessageHandler;
        private InteractionHandler _interactionHandler;
        private PagedMessageHandler _pagedMessageHandler;

        // TODO: THIS SUCKS, DO IT ANOTHER WAY AT SOME POINT
        public async Task InvokePageForEmbedUpdatedEvent(ulong id) => await PageForEmbedUpdated!.Invoke(id);
        public event Func<ulong, Task>? PageForEmbedUpdated;
        public Dictionary<ulong, int> CurrentPageForEmbed { get; set; } = new();
        public Dictionary<ulong, ButtonType> LastButtonIdClickedForEmbeded { get; set; } = new();

        public BotClient(CommandLineArgs args)
        {
            _args = args;
            Instance = this;
            SetupApp();
        }

        public async Task StartAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var discordOptions = scope.ServiceProvider.GetRequiredService<IOptions<DiscordOptions>>();

                new LoggingService(_client, _cmds, _interactService);

                await _client.LoginAsync(TokenType.Bot, discordOptions.Value.Token);
                await _client.StartAsync();

                await Task.Delay(-1); // Block               
            }
        }

        private void SetupApp()
        {
            SetupConfiguration();
            SetupLogging();
            SetupServiceProvider();
            SetupClient();
        }

        private void SetupLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_config)
                .CreateLogger();
        }

        private void SetupConfiguration()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        private void SetupServiceProvider()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(builder =>
            {
                builder.AddSerilog();
            });

            serviceCollection
                .AddAppOption<DatabaseOptions>(_config)
                .AddAppOption<DiscordOptions>(_config)
                .AddAppOption<OsuWebApiOptions>(_config)
                .AddAppOption<CacheOptions>(_config)
                .AddScoped<IConnectionMultiplexer, ConnectionMultiplexer>(sp =>
                {
                    var cacheOptions = sp.GetRequiredService<IOptions<CacheOptions>>();
                    return ConnectionMultiplexer.Connect(cacheOptions.Value.RedisConnection);
                })
                .AddSingleton<IOsuWebClient, OsuWebClient>()
                .AddSingleton<IRedisService>(sp =>
                {
                    return _args.DisableCaching ?
                        new DisabledRedisService():
                        new RedisService(sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RedisService>>(), sp.GetRequiredService<IOptions<CacheOptions>>(), sp.GetRequiredService<IConnectionMultiplexer>());     
                })
                .AddSingleton<IPlaycountFetchJobQueue, PlaycountFetchJobQueue>()
                .AddScoped<IDatabase, MySqlDatabase>()
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IBeatmapRepository, BeatmapRepository>()
                .AddScoped<IOsuWebClient, OsuWebClient>()
                .AddScoped<IUserSearch, UserSearch>()
                .AddScoped<IBotCommandService, BotCommandService>()
                .AddSingleton<ICommandFactory, CommandFactory>()
                .AddSingleton<ITransactionFactory, TransactionFactory>();

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void SetupClient()
        {
            DiscordSocketConfig discordSocketConfig = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(discordSocketConfig);

            SetupMessageHandlers();

            _client.Ready += OnClientReady;
            _client.InteractionCreated += _interactionHandler.OnInteraction;
            _client.MessageCommandExecuted += _interactionHandler.OnInteraction;
            _client.UserCommandExecuted += _interactionHandler.OnInteraction;
            _client.ButtonExecuted += _pagedMessageHandler.OnButtonClick;
            _client.ModalSubmitted += _pagedMessageHandler.OnModalSubmit;
            _client.MessageReceived += _simpleMessageHandler.OnMsgReceived;
        }

        private void SetupMessageHandlers()
        {
            _cmds = new CommandService();
            _simpleMessageHandler = new SimpleMessageHandler(_client, _serviceProvider, _cmds);

            _interactService = new InteractionService(_client.Rest);
            _interactionHandler = new InteractionHandler(_client, _serviceProvider, _interactService);

            _pagedMessageHandler = new PagedMessageHandler();
        }

        private async Task OnClientReady()
        {
            ulong guildId = 845680976243982356;

            try
            {
                var assembly = Assembly.GetEntryAssembly();
                await _interactService.AddModulesAsync(assembly, _serviceProvider);
                await _interactService.RegisterCommandsToGuildAsync(guildId);
                await _cmds.AddModulesAsync(assembly, _serviceProvider);
            }
            catch (HttpException e)
            {
                Log.Error(e, "Error while client was getting ready");
            }
        }
    }
}
