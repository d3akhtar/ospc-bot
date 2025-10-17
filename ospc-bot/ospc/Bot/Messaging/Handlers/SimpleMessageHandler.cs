using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using OSPC.Infrastructure.Database.Repository;
using OSPC.Utils;

using Serilog;

namespace OSPC.Bot.Messaging.Handlers
{
    public class SimpleMessageHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _serviceProvider;
        private readonly CommandService _cmds;

        public SimpleMessageHandler(DiscordSocketClient client, IServiceProvider serviceProvider, CommandService cmds)
        {
            _client = client;
            _serviceProvider = serviceProvider;
            _cmds = cmds;
        }

        public async Task OnMsgReceived(SocketMessage message)
        {
            if (message.Author.IsBot)
                return;

            _ = Task.Run(async () => await HandleBeatmapLinks(message));

            int argPos = 0;
            var userMessage = message as SocketUserMessage;
            if (userMessage.HasCharPrefix('=', ref argPos))
            {
                await _cmds.ExecuteAsync(
                    context: new SocketCommandContext(_client, userMessage),
                    argPos: argPos,
                    services: _serviceProvider
                );
            }
        }

        private async Task HandleBeatmapLinks(SocketMessage message)
        {
            var match = RegexPatterns.OsuBeatmapLinkRegex.Match(message.Content);
            if (match.Success)
            {
                Log.Information("Received beatmap link: {Link}", message.Content);

                int beatmapId = int.Parse(match.Groups[2].Value);

                using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var beatmapRepo = scope.ServiceProvider.GetRequiredService<IBeatmapRepository>();
                    await beatmapRepo.UpdateReferencedBeatmapIdForChannel(message.Channel.Id, beatmapId);
                }
            }
        }
    }
}