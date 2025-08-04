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
            => await ReplyAsync(embed: (await _botCmds.GetPlaycount(string.Empty, -1, Context.GetOsuContext())).Embed);

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

            await ReplyAsync(embed: await _botCmds.GetPlaycount(username, beatmapId, Context.GetOsuContext()));
        }

        [Command("pc")]
        [Summary("Get the playcount on a beatmap for a user")]
        public async Task GetPlaycount(string username, int beatmapId)
            => await ReplyAsync(embed: await _botCmds.GetPlaycount(username, beatmapId, Context.GetOsuContext()));

        [Command("pc")]
        [Summary("Get the playcount on a beatmap for a user")]
        public async Task GetPlaycount(int beatmapId)
            => await ReplyAsync(embed: await _botCmds.GetPlaycount(string.Empty, beatmapId, Context.GetOsuContext()));

        [Command("search")]
        [Summary("Search for beatmaps in most-played")]
        public async Task Search([Remainder]string input)
        {
            SearchParams? searchParams = SearchParams.GetSearchParamsFromInput(input);
            if (searchParams == null) {
                await ReplyAsync(embed: Embeded.BuildErrorEmbed("Invalid input."));
                return;
            }

            (Embed embed, MessageComponent? components, PlaycountEmbedContext embedContext) 
                = await _botCmds.Search(searchParams, Context.GetOsuContext());

            if (components == null) {
                await ReplyAsync(embed: embed);
                return;
            }

            embedContext.Message = await ReplyAsync (
                embed: embed,
                components: components
            );

            Embeded.CreatePlaycountListEmbed(embedContext);
        }

        private async Task pageForEmbedUpdated(ulong id)
            => await Embeded.PageForEmbedUpdated(_osuWebClient, _beatmapRepo, id);

        [Command("most-played")]
        public async Task MostPlayed(string username = "")
        {
            (Embed embed, MessageComponent? buttons, PlaycountEmbedContext embedCtx) = await _botCmds.GetMostPlayed(username, Context.GetOsuContext());
            if (buttons == null) {
                await ReplyAsync(embed: embed);
                return;
            }

            embedCtx.Message = await ReplyAsync (
                embed: embed,
                components: buttons
            );

            Embeded.CreatePlaycountListEmbed(embedCtx);
        }
    }
}
