using Discord;
using Discord.Commands;
using OSPC.Bot.Command;
using OSPC.Bot.Component;
using OSPC.Bot.Context;
using OSPC.Infrastructure.Database.Repository;
using OSPC.Infrastructure.Http;
using OSPC.Utils;

namespace OSPC.Bot.Module.Info
{
    public class TrackInfoModule : ModuleBase<SocketCommandContext>
    {
        private readonly IBeatmapRepository _beatmapRepo;
        private readonly IOsuWebClient _osuWebClient;
        private readonly IBotCommandService _botCmds;

        public TrackInfoModule(IBotCommandService botCmds, IBeatmapRepository beatmapRepo, IOsuWebClient osuWebClient)
        {
            BotClient.Instance.PageForEmbedUpdated += pageForEmbedUpdated;
            _botCmds = botCmds;
            _beatmapRepo = beatmapRepo;
            _osuWebClient = osuWebClient;
        }

        [Command("pc")]
        [Summary("Get the playcount on a beatmap for a user")]
        public async Task GetPlaycount(string username, string beatmapLink)
        {
            var result = await _botCmds.GetPlaycount(Context.GetOsuContext(), username, beatmapLink);
            await ReplyAsync(embed: result.Embed);
        }

        // TODO: FIX THE MAGIC STUFF WTF MAN
        [Command("pc")]
        [Summary("Get the playcount on a beatmap for a user")]
        public async Task GetPlaycount()
        {
            var result = await _botCmds.GetPlaycount(Context.GetOsuContext(), string.Empty, -1);
            await ReplyAsync(embed: result.Embed);
        }

        [Command("pc")]
        [Summary("Get the playcount on a beatmap for a user")]
        public async Task GetPlaycount(string usernameOrBeatmap)
        {
            string username = ""; int beatmapId = -1;
            var match = RegexPatterns.OsuBeatmapLinkRegex.Match(usernameOrBeatmap);
            if (match.Success) beatmapId = int.Parse(match.Groups[2].Value);
            else if (RegexPatterns.StrictUsernameRegex.IsMatch(usernameOrBeatmap)) { 
                username = usernameOrBeatmap;
            } else {
                await ReplyAsync(embed: Embeded.BuildErrorEmbed("Invalid parameters"));
                return;
            }

            var result = await _botCmds.GetPlaycount(Context.GetOsuContext(), username, beatmapId);
            await ReplyAsync(embed: result.Embed);
        }

        [Command("pc")]
        [Summary("Get the playcount on a beatmap for a user")]
        public async Task GetPlaycount(string username, int beatmapId)
        {
            var result = await _botCmds.GetPlaycount(Context.GetOsuContext(), username, beatmapId);
            await ReplyAsync(embed: result.Embed);
        }

        [Command("pc")]
        [Summary("Get the playcount on a beatmap for a user")]
        public async Task GetPlaycount(int beatmapId)
        {
            var result = await _botCmds.GetPlaycount(Context.GetOsuContext(), string.Empty, beatmapId);
            await ReplyAsync(embed: result.Embed);
        }

        [Command("search")]
        [Summary("Search for beatmaps in most-played")]
        public async Task Search([Remainder]string input)
        {
            SearchParams? searchParams = SearchParams.GetSearchParamsFromInput(input);
            if (searchParams == null) {
                await ReplyAsync(embed: Embeded.BuildErrorEmbed("Invalid input."));
                return;
            }

                
            var result = await _botCmds.Search(Context.GetOsuContext(), searchParams);

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

        private async Task pageForEmbedUpdated(ulong id)
            => await Embeded.PageForEmbedUpdated(_osuWebClient, _beatmapRepo, id);

        [Command("most-played")]
        public async Task MostPlayed(string username = "")
        {
            var result = await _botCmds.GetMostPlayed(Context.GetOsuContext(), username);
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
