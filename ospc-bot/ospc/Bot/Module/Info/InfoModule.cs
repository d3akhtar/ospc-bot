using Discord.Commands;

using Microsoft.Extensions.Logging;

using OSPC.Bot.Command.Result;
using OSPC.Bot.Component;
using OSPC.Bot.Messaging.Handlers;
using OSPC.Domain.Common;

using PagedReply = OSPC.Bot.Command.Result.PagedReply;

namespace OSPC.Bot.Module.Info
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
#pragma warning disable CS8625
        protected ILogger _logger = default;
        protected PagedMessageHandler _pagedMessageHandler = default;
#pragma warning restore CS8625

        public async Task ReplyAsync(Reply result)
        {
            if (result is PagedReply searchResult)
                await ReplyPagedAsync(searchResult);
            else
                await ReplySingleAsync(result);
        }

        public async Task ReplyErrorAsync(Error error)
        {
            _logger.LogInformation("Replying to user with error: {@Error}", error);
            await ReplyAsync(embed: EmbededUtils.BuildErrorEmbed(error));
        }

        private async Task ReplySingleAsync(Reply result)
        {
            _logger.LogDebug("Responding with command result: {CommandResult}", result);
            await ReplyAsync(embed: result.Embed);
        }

        private async Task ReplyPagedAsync(PagedReply result)
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

            _pagedMessageHandler.CreateActivePagedMessage(result.Context);
        }
    }
}