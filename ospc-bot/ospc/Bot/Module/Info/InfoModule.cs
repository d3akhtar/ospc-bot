using Discord.Commands;

using Microsoft.Extensions.Logging;

using OSPC.Bot.Command.Result;
using OSPC.Bot.Component;
using OSPC.Utils;

using SearchResult = OSPC.Bot.Command.Result.SearchResult;

namespace OSPC.Bot.Module.Info
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
#pragma warning disable CS8625
        protected ILogger _logger = default;

        public async Task ReplyBotCommandResultAsync(CommandResult result)
        {
            if (result is SearchResult searchResult)
                await ReplySearchResultAsync(searchResult);
            else
                await ReplyCommandResultAsync(result);
        }

        public async Task ReplyErrorAsync(Error error)
        {
            _logger.LogInformation("Replying to user with error: {@Error}", error);
            await ReplyAsync(embed: Embeded.BuildErrorEmbed(error));
        }

        private async Task ReplyCommandResultAsync(CommandResult result)
        {
            _logger.LogDebug("Responding with command result: {CommandResult}", result);
            await ReplyAsync(embed: result.Embed);
        }

        private async Task ReplySearchResultAsync(SearchResult result)
        {
            _logger.LogDebug("Responding with search result: {SearchResult}", result);

            if (!result.Successful)
            {
                await ReplyAsync(embed: result.Embed);
                return;
            }

            result.Context!.Message = await ReplyAsync(
                embed: result.Embed,
                components: result.Components
            );

            Embeded.CreatePlaycountListEmbed(result.Context);
        }
    }
}