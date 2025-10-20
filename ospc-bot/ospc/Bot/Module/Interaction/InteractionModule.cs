using Discord.Interactions;

using Microsoft.Extensions.Logging;

using OSPC.Bot.Command.Result;
using OSPC.Bot.Component;
using OSPC.Bot.Messaging.Handlers;
using OSPC.Domain.Common;

using PagedReply = OSPC.Bot.Command.Result.PagedReply;

namespace OSPC.Bot.Module.Interaction
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
#pragma warning disable CS8625
        protected ILogger _logger = default;
        protected PagedMessageHandler _pagedMessageHandler = default;
#pragma warning restore CS8625

        public async Task RespondAsync(Reply result)
        {
            if (result is PagedReply searchResult)
                await RespondPagedAsync(searchResult);
            else
                await RespondSingleAsync(result);
        }

        public async Task RespondErrorAsync(Error error)
            => await RespondAsync(embed: EmbededUtils.BuildErrorEmbed(error));

        private async Task RespondSingleAsync(Reply result)
        {
            _logger.LogInformation("Responding with command result: {@CommandResult}", result);
            await RespondAsync(embed: result.Embed);
        }

        private async Task RespondPagedAsync(PagedReply result)
        {
            _logger.LogInformation("Responding with search result: {@SearchResult}", result);

            await DeferAsync();

            if (!result.Successful)
            {
                await ReplyAsync(embed: result.Embed);
                return;
            }

            result.Context!.Message = await ReplyAsync(
                embed: result.Embed,
                components: result.Components
            );

            _pagedMessageHandler.CreateActivePagedMessage(result.Context);

            await DeleteOriginalResponseAsync();
        }
    }
}