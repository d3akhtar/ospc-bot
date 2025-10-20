using System.Reflection;

using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using OSPC.Bot.Logging;
using OSPC.Bot.Messaging.Handlers;
using OSPC.Bot.Enums;
using OSPC.Domain.Options;

using Serilog;

namespace OSPC.Bot
{
    public class BotClient
    {
        public static BotClient Instance { get; private set; }
        
        private DiscordSocketClient _client;
        private CommandService _cmds;
        private InteractionService _interactService;
        private IServiceProvider _serviceProvider;

        private SimpleMessageHandler _simpleMessageHandler;
        private InteractionHandler _interactionHandler;
        private PagedMessageHandler _pagedMessageHandler;

        // TODO: THIS SUCKS, DO IT ANOTHER WAY AT SOME POINT
        public Dictionary<ulong, int> CurrentPageForEmbed { get; set; } = new();
        public Dictionary<ulong, ButtonType> LastButtonIdClickedForEmbeded { get; set; } = new();

        public BotClient(IServiceCollection serviceCollection)
        {
            Instance = this;
            _serviceProvider = serviceCollection.BuildServiceProvider();
            SetupClient();
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
