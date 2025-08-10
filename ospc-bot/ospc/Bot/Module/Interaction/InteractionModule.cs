using Discord.Commands;
using Discord.Interactions;
using OSPC.Bot.Command.Result;
using OSPC.Bot.Component;
using SearchResult = OSPC.Bot.Command.Result.SearchResult;

namespace OSPC.Bot.Module.Interaction
{
	public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
	{
		public async Task RespondBotCommandResultAsync(CommandResult result)
		{
			if (result is SearchResult searchResult) await RespondSearchResultAsync(searchResult);
			else await RespondCommandResultAsync(result);
		}

		public async Task RespondErrorAsync(string message)
			=> await RespondAsync(embed: Embeded.BuildErrorEmbed(message));

		private async Task RespondCommandResultAsync(CommandResult result)
			=> await RespondAsync(embed: result.Embed);
		
		private async Task RespondSearchResultAsync(SearchResult result)
		{
            await DeferAsync();

            if (!result.Successful) {
                await ReplyAsync(embed: result.Embed);
                return;
            }

            result.Context!.Message = await ReplyAsync (
                embed: result.Embed,
                components: result.Components
            );

            Embeded.CreatePlaycountListEmbed(result.Context);

            await DeleteOriginalResponseAsync();
		}		
	}
}
