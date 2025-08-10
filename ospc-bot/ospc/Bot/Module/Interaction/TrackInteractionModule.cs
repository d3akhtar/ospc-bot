using Discord;
using Discord.Interactions;
using OSPC.Bot.Command;
using OSPC.Bot.Component;
using OSPC.Bot.Context;
using OSPC.Infrastructure.Database.Repository;
using OSPC.Infrastructure.Http;
using OSPC.Utils;

namespace OSPC.Bot.Module.Interaction
{
    public class TrackInteractionModule : InteractionModule
    {
        private const int LIMIT = 25;
        private readonly IBeatmapRepository _beatmapRepo;
        private readonly IOsuWebClient _osuWebClient;
        private readonly IBotCommandService _botCmds;

        public TrackInteractionModule(IBeatmapRepository beatmapRepo, IOsuWebClient osuWebClient, IBotCommandService botCmds)
        {
            _beatmapRepo = beatmapRepo;
            _osuWebClient = osuWebClient;
            BotClient.Instance.PageForEmbedUpdated += pageForEmbedUpdated;
            _botCmds = botCmds;
        }

        private async Task pageForEmbedUpdated(ulong id)
            => await Embeded.PageForEmbedUpdated(_osuWebClient, _beatmapRepo, id);

        [SlashCommand("most-played", "Get most played beatmaps for a user")]
        public async Task GetMostPlayed(string username)
            => await RespondBotCommandResultAsync(await _botCmds.GetMostPlayed(Context.GetOsuContext(), username));

        [SlashCommand("playcount", "Get the playcount on a beatmap for a user")]
        public async Task GetPlaycount(string username = "", int beatmapId = -1)
            => await RespondBotCommandResultAsync(await _botCmds.GetPlaycount(Context.GetOsuContext(), username, beatmapId));

        [SlashCommand("search", "Search for beatmaps in most played")]
        public async Task Search(
            string username = "",
            int playcount = -1,
            Comparison comparison = Comparison.Equal,
            string query = "", 
            string artist = "",
            string title = "",
            bool exact = false,
            [Summary(name:"beatmap-filter", description: "Filter beatmaps similar to how you would do so in osu. Use the /help command for details")]
            string beatmapFilterStr = "")
        {
            (bool success, BeatmapFilter? beatmapFilter, beatmapFilterStr) = BeatmapFilter.ParseBeatmapFilter(beatmapFilterStr);
            if (!success) {
                await RespondErrorAsync("Failed to parse beatmap filters.");
                return;
            }

            if (!string.IsNullOrEmpty(username) && !RegexPatterns.StrictUsernameRegex.IsMatch(username)) await RespondErrorAsync("Invalid username format!");
            else if (UnfilledQuery(query, artist, title, playcount, comparison)) await RespondErrorAsync("Either fill only the query, or fill one or both of artist and title");  
            else {
                SearchParams searchParams = new() {
                     Username = username,
                     Query = query,
                     Artist = artist,
                     Title = title,
                     Exact = exact,
                     BeatmapFilter = beatmapFilter!
                };

                await RespondBotCommandResultAsync(await _botCmds.Search(Context.GetOsuContext(), searchParams));
            }
        }

        private bool UnfilledQuery(string query, string artist, string title, int playcount, Comparison comparison)
            => string.IsNullOrEmpty(query) && query == artist && artist == title && playcount == -1 && comparison == Comparison.None;
    }
}
