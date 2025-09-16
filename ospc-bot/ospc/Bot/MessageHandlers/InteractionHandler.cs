using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using Serilog;

namespace OSPC.Bot.MessageHandlers
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _serviceProvider;
        private readonly InteractionService _interactionService;

        public InteractionHandler(DiscordSocketClient client, IServiceProvider serviceProvider, InteractionService interactionService)
        {
            _client = client;
            _serviceProvider = serviceProvider;
            _interactionService = interactionService;
        }

        public async Task OnInteraction(SocketInteraction interaction)
        {
            Log.Information<SocketInteraction>("Received interaction: {Interaction}", interaction);

            var scope = _serviceProvider.CreateScope();
            var ctx = new SocketInteractionContext(_client, interaction);
            await _interactionService.ExecuteCommandAsync(ctx, scope.ServiceProvider);
        }
    }
}