using Discord.Commands;

using Microsoft.Extensions.Logging;

using OSPC.Bot.Component;
using OSPC.Bot.Extensions;
using OSPC.Bot.Service;
using OSPC.Domain.Common;
using OSPC.Infrastructure.Database.Repository;
using OSPC.Infrastructure.Http;
using OSPC.Utils;
using OSPC.Parsing.ParsedObjects;
using OSPC.Domain.Constants;

namespace OSPC.Bot.Module.Info
{
    public class TrackInfoModule : InfoModule
    {
        private readonly IBeatmapRepository _beatmapRepo;
        private readonly IOsuWebClient _osuWebClient;
        private readonly IBotCommandService _botCmds;

        public TrackInfoModule(ILogger<TrackInfoModule> logger, IBotCommandService botCmds, IBeatmapRepository beatmapRepo, IOsuWebClient osuWebClient)
        {
            _logger = logger;
            BotClient.Instance.PageForEmbedUpdated += PageForEmbedUpdated;
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
            => await ReplyBotCommandResultAsync(await _botCmds.GetPlaycount(Context.GetOsuContext(), Unspecified.User, Unspecified.Beatmap));

        [Command("pc")]
        [Summary("Get the playcount on a beatmap for a user")]
        public async Task GetPlaycount(string usernameOrBeatmap)
        {
            if (TryParseUsernameOrBeatmap(usernameOrBeatmap, out string username, out int beatmapId))
                await ReplyBotCommandResultAsync(await _botCmds.GetPlaycount(Context.GetOsuContext(), username, beatmapId));
            else
                await ReplyErrorAsync(Errors.Parsing("Invalid username or beatmap link specified, see more info about username/beatmap link format with `=help pc`"));
        }

        [Command("pc")]
        [Summary("Get the playcount on a beatmap for a user")]
        public async Task GetPlaycount(string username, int beatmapId)
            => await ReplyBotCommandResultAsync(await _botCmds.GetPlaycount(Context.GetOsuContext(), username, beatmapId));

        [Command("pc")]
        [Summary("Get the playcount on a beatmap for a user")]
        public async Task GetPlaycount(int beatmapId)
            => await ReplyBotCommandResultAsync(await _botCmds.GetPlaycount(Context.GetOsuContext(), Unspecified.User, beatmapId));

        [Command("search")]
        [Summary("Search for beatmaps in most-played")]
        public async Task Search([Remainder] string input)
        {
            var result = SearchParams.Parse(input);
            if (!result.Successful)
                await ReplyErrorAsync(result.Error!);
            else
                await ReplyBotCommandResultAsync(await _botCmds.Search(Context.GetOsuContext(), result.Value!));
        }

        private async Task PageForEmbedUpdated(ulong id)
            => await EmbededUtils.PageForEmbedUpdated(_osuWebClient, _beatmapRepo, id);

        [Command("most-played")]
        public async Task MostPlayed(string username = Unspecified.User)
            => await ReplyBotCommandResultAsync(await _botCmds.GetMostPlayed(Context.GetOsuContext(), username));

        private bool TryParseUsernameOrBeatmap(string usernameOrBeatmap, out string username, out int beatmapId)
        {
            username = Unspecified.User;
            beatmapId = Unspecified.Beatmap;

            var match = RegexPatterns.OsuBeatmapLinkRegex.Match(usernameOrBeatmap);

            if (match.Success)
                beatmapId = int.Parse(match.Groups[2].Value);
            else if (RegexPatterns.StrictUsernameRegex.IsMatch(usernameOrBeatmap))
                username = usernameOrBeatmap;
            else
                return false;

            return true;
        }
    }
}
