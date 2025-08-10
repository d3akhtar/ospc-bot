using Discord.Commands;
using OSPC.Bot.Command.Result;
using OSPC.Bot.Component;
using SearchResult = OSPC.Bot.Command.Result.SearchResult;

namespace OSPC.Bot.Module.Info
{
	public class InfoModule : ModuleBase<SocketCommandContext>
	{
		public async Task ReplyBotCommandResultAsync(CommandResult result)
		{
			if (result is SearchResult searchResult) await ReplySearchResultAsync(searchResult);
			else await ReplyCommandResultAsync(result);
		}

		public async Task ReplyErrorAsync(string message)
			=> await ReplyAsync(embed: Embeded.BuildErrorEmbed(message));		

		private async Task ReplyCommandResultAsync(CommandResult result)
		{
			await ReplyAsync(embed: result.Embed);
		}
		
		private async Task ReplySearchResultAsync(SearchResult result)
		{
			if (!result.Successful) {
				await ReplyAsync(embed: result.Embed);
				return;
			}
			
            result.Context!.Message = await ReplyAsync (
                embed: result.Embed,
                components: result.Components
            );

            Embeded.CreatePlaycountListEmbed(result.Context);
		}
	} 
}
