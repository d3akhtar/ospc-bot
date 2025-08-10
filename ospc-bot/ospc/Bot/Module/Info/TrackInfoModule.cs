using Discord;
using Discord.Commands;
using OSPC.Bot.Command;
using OSPC.Bot.Component;
using OSPC.Bot.Context;
using OSPC.Domain.Model;
using OSPC.Infrastructure.Database.Repository;
using OSPC.Infrastructure.Http;
using OSPC.Utils;

namespace OSPC.Bot.Module.Info
{
    public class TrackInfoModule : InfoModule
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

        [Command("pc")]
        [Summary("Get the playcount on a beatmap for a user")]
        public async Task GetPlaycount()
            => await ReplyBotCommandResultAsync(await _botCmds.GetPlaycount(Context.GetOsuContext(), User.Unspecified, Beatmap.Unspecified));

        [Command("pc")]
        [Summary("Get the playcount on a beatmap for a user")]
        public async Task GetPlaycount(string usernameOrBeatmap)
        {
            if (TryParseUsernameOrBeatmap(usernameOrBeatmap, out string username, out int beatmapId))
                await ReplyBotCommandResultAsync(await _botCmds.GetPlaycount(Context.GetOsuContext(), username, beatmapId));
            else
                await ReplyErrorAsync("Invalid parameters");
        }

        [Command("pc")]
        [Summary("Get the playcount on a beatmap for a user")]
        public async Task GetPlaycount(string username, int beatmapId)
            => await ReplyBotCommandResultAsync(await _botCmds.GetPlaycount(Context.GetOsuContext(), username, beatmapId));

        [Command("pc")]
        [Summary("Get the playcount on a beatmap for a user")]
        public async Task GetPlaycount(int beatmapId)
            => await ReplyBotCommandResultAsync(await _botCmds.GetPlaycount(Context.GetOsuContext(), User.Unspecified, beatmapId));

        [Command("search")]
        [Summary("Search for beatmaps in most-played")]
        public async Task Search([Remainder]string input)
        {
            SearchParams? searchParams = SearchParams.GetSearchParamsFromInput(input);
            if (searchParams == null) {
                await ReplyErrorAsync("Invalid input.");
                return;
            }

            await ReplyBotCommandResultAsync(await _botCmds.Search(Context.GetOsuContext(), searchParams));
        }

        private async Task pageForEmbedUpdated(ulong id)
            => await Embeded.PageForEmbedUpdated(_osuWebClient, _beatmapRepo, id);

        [Command("most-played")]
        public async Task MostPlayed(string username = "")
            => await ReplyBotCommandResultAsync(await _botCmds.GetMostPlayed(Context.GetOsuContext(), username));

        private bool TryParseUsernameOrBeatmap(string usernameOrBeatmap, out string username, out int beatmapId)
        {
            username = User.Unspecified;
            beatmapId = Beatmap.Unspecified;

            var match = RegexPatterns.OsuBeatmapLinkRegex.Match(usernameOrBeatmap);

            if (match.Success) beatmapId = int.Parse(match.Groups[2].Value);
            else if (RegexPatterns.StrictUsernameRegex.IsMatch(usernameOrBeatmap)) username = usernameOrBeatmap;
            else return false;

            return true;
        }
    }
}
