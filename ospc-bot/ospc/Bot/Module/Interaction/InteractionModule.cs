using Discord.Interactions;

using Microsoft.Extensions.Logging;

using OSPC.Bot.Command.Result;
using OSPC.Bot.Component;
using OSPC.Domain.Common;

using PagedReply = OSPC.Bot.Command.Result.PagedReply;

namespace OSPC.Bot.Module.Interaction
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
#pragma warning disable CS8625
        protected ILogger _logger = default;

        public async Task RespondBotCommandResultAsync(Reply result)
        {
            if (result is PagedReply searchResult)
                await RespondSearchResultAsync(searchResult);
            else
                await RespondCommandResultAsync(result);
        }

        public async Task RespondErrorAsync(Error error)
            => await RespondAsync(embed: EmbededUtils.BuildErrorEmbed(error));

        private async Task RespondCommandResultAsync(Reply result)
        {
            _logger.LogInformation("Responding with command result: {@CommandResult}", result);
            await RespondAsync(embed: result.Embed);
        }

        private async Task RespondSearchResultAsync(PagedReply result)
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

            EmbededUtils.CreatePlaycountListEmbed(result.Context);

            await DeleteOriginalResponseAsync();
        }
    }
}