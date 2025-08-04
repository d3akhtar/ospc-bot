using System.Reflection;
using System.Text.Json;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using OSPC.Bot.Command;
using OSPC.Bot.Component;
using OSPC.Bot.Logging;
using OSPC.Bot.Search.UserSearch;
using OSPC.Infrastructure.Caching;
using OSPC.Infrastructure.Database;
using OSPC.Infrastructure.Database.Repository;
using OSPC.Infrastructure.Http;
using OSPC.Infrastructure.Job;
using OSPC.Utils;

namespace OSPC.Bot
{
    public class BotClient
    {
        public static BotClient Instance { get; private set; }
        private readonly DiscordSocketClient _client;
        private readonly CommandService _cmds;
        private readonly InteractionService _interactService;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _token;

        public event Func<ulong,Task>? PageForEmbedUpdated;
        public Dictionary<ulong, int> CurrentPageForEmbed { get; set; } = new();
        public Dictionary<ulong, ButtonType> LastButtonIdClickedForEmbeded { get; set; } = new();

        public BotClient()
        {
            Instance = this;
            
            DiscordSocketConfig config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(config);
            
            _client.Ready += onClientReady;
            _client.InteractionCreated += onInteraction;
            _client.MessageCommandExecuted += onInteraction;
            _client.UserCommandExecuted += onInteraction;
            _client.ButtonExecuted += onButtonClick;
            _client.ModalSubmitted += onModalSubmit;
            _client.MessageReceived += onMsgReceived;

            _interactService = new InteractionService(_client.Rest);
            _cmds = new CommandService();

            Settings settings = Settings.LoadFromFile("../settings.json");
            _token = settings.Token;

            new LoggingService(_client, _cmds, _interactService, settings.Logging);

            _serviceProvider = new ServiceCollection()
                .AddSingleton(settings)
                .AddSingleton<IOsuWebClient, OsuWebClient>()
                .AddSingleton<IRedisService, RedisService>()
                .AddSingleton<PlaycountFetchJobQueue>()
                .AddScoped<DbContext>()
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IBeatmapRepository, BeatmapRepository>()
                .AddScoped<IOsuWebClient, OsuWebClient>()
                .AddScoped<IUserSearch, UserSearch>()
                .AddScoped<IBotCommandService, BotCommandService>()
                .BuildServiceProvider();
        }

        private async Task onModalSubmit(SocketModal modal)
        {
            ulong msgId = modal.Message.Id;
            Console.WriteLine($"msgId: {msgId}");
            try {
                switch (modal.Data.CustomId) {
                    case "target_page_number": {
                        await modal.DeferAsync();
                        CurrentPageForEmbed[msgId] = int.Parse(modal.Data.Components.First(x => x.CustomId == "page_number").Value);
                        LastButtonIdClickedForEmbeded[msgId] = ButtonType.NextPage;
                        Embeded.PauseTimer(modal.Message);
                        await PageForEmbedUpdated!.Invoke(msgId);
                        Embeded.ResetTimer(modal.Message);
                        break;
                    }
                    default: throw new Exception("Unknown modal");
                }
            } catch (TimeoutException) {
                Console.WriteLine($"Msg: {msgId} not acknowledged yet");
                switch (LastButtonIdClickedForEmbeded[msgId]) {
                    case ButtonType.Unknown: break;
                    case ButtonType.PreviousPage: {
                        CurrentPageForEmbed[msgId]++;
                        LastButtonIdClickedForEmbeded[msgId] = ButtonType.Unknown;
                        break;
                    }
                    case ButtonType.NextPage: {
                        CurrentPageForEmbed[msgId]--;
                        LastButtonIdClickedForEmbeded[msgId] = ButtonType.Unknown;
                        break;
                    }
                }
            }
        }

        private async Task onMsgReceived(SocketMessage message)
        {
            if (message.Author.IsBot) return;

            _ = Task.Run(async () => await handleBeatmapLinks(message));

            int argPos = 0;
            var userMessage = message as SocketUserMessage;
            if (userMessage.HasCharPrefix('=', ref argPos)) {
                await _cmds.ExecuteAsync(
                    context: new SocketCommandContext(_client, userMessage),
                    argPos: argPos,
                    services: _serviceProvider
                );
            }
        }

        private async Task handleBeatmapLinks(SocketMessage message)
        {
            var match = RegexPatterns.OsuBeatmapLinkRegex.Match(message.Content);
            if (match.Success){
                int beatmapId = int.Parse(match.Groups[2].Value);
                using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var beatmapRepo = scope.ServiceProvider.GetRequiredService<IBeatmapRepository>();
                    await beatmapRepo.UpdateReferencedBeatmapIdForChannel(message.Channel.Id, beatmapId);
                }
            }
        }

        private async Task onButtonClick(SocketMessageComponent component)
        {
            ulong msgId = component.Message.Id;
            Console.WriteLine($"msgId: {msgId}");
            try {
                switch (component.Data.CustomId) {
                    case "first_page":
                        await component.DeferAsync();
                        CurrentPageForEmbed[msgId] = 1;
                        LastButtonIdClickedForEmbeded[msgId] = ButtonType.FirstPage;
                        break;
                    case "last_page":
                        await component.DeferAsync();
                        CurrentPageForEmbed[msgId] = Embeded.ActiveEmbeds[msgId].TotalPages;
                        LastButtonIdClickedForEmbeded[msgId] = ButtonType.LastPage;
                        break;
                    case "choose_page":
                        await component.RespondWithModalAsync(modal:  
                        new ModalBuilder()
                            .WithTitle("Enter ")
                            .WithCustomId("target_page_number")
                            .AddTextInput("Enter Page Number", "page_number", placeholder:"1").Build());
                        break;
                    case "next_page":
                        await component.DeferAsync();
                        CurrentPageForEmbed[msgId]++;
                        LastButtonIdClickedForEmbeded[msgId] = ButtonType.NextPage;
                        break;
                    case "prev_page":
                        await component.DeferAsync();
                        CurrentPageForEmbed[msgId] = CurrentPageForEmbed[msgId] <= 1 ? 1:CurrentPageForEmbed[msgId]-1;
                        LastButtonIdClickedForEmbeded[msgId] = ButtonType.PreviousPage;
                        break;
                }
                Embeded.PauseTimer(component.Message);
                await PageForEmbedUpdated!.Invoke(msgId);
                Embeded.ResetTimer(component.Message);
            } catch (TimeoutException) {
                Console.WriteLine($"Msg: {msgId} not acknowledged yet");
                switch (LastButtonIdClickedForEmbeded[msgId]) {
                    case ButtonType.Unknown: break;
                    case ButtonType.PreviousPage: {
                        CurrentPageForEmbed[msgId]++;
                        LastButtonIdClickedForEmbeded[msgId] = ButtonType.Unknown;
                        break;
                    }
                    case ButtonType.NextPage: {
                        CurrentPageForEmbed[msgId]--;
                        LastButtonIdClickedForEmbeded[msgId] = ButtonType.Unknown;
                        break;
                    }
                }
            }
        }

        private async Task onClientReady()
        {
            ulong guildId = 845680976243982356;

            try {
                var assembly = Assembly.GetEntryAssembly();
                await _interactService.AddModulesAsync(assembly, _serviceProvider);
                await _interactService.RegisterCommandsToGuildAsync(guildId);
                await _cmds.AddModulesAsync(assembly, _serviceProvider);
            } catch (HttpException e){
                var json = JsonSerializer.Serialize(e.Errors);
                Console.WriteLine(json);
            }
        }

        public async Task StartAsync()
        {
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();

            await Task.Delay(-1); // Block
        }

        private async Task onInteraction(SocketInteraction interaction)
        {
            var scope = _serviceProvider.CreateScope();
            var ctx = new SocketInteractionContext(_client, interaction);
            await _interactService.ExecuteCommandAsync(ctx, scope.ServiceProvider);
        }
    }
}
