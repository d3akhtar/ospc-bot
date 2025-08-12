using Discord;
using OSPC.Bot.Context;
using OSPC.Domain.Model;
using OSPC.Infrastructure.Database.Repository;
using OSPC.Infrastructure.Http;

namespace OSPC.Bot.Component
{
    public static class Embeded
    {
        private const int LIMIT = 25;
        private const int BUTTON_DELETE_TIME = 30;
        public static Dictionary<ulong, PlaycountEmbedContext> ActiveEmbeds = new();
        public static Dictionary<ulong, Timer> ButtonDeleteTimers = new();

        public static void CreatePlaycountListEmbed(PlaycountEmbedContext ctx)
        {
            ActiveEmbeds.Add(ctx.Message.Id, ctx);
            StartTask(ctx.Message);
        }
        public static void StartTask(IUserMessage message) {
            BotClient.Instance.CurrentPageForEmbed.Add(message.Id, 1);
            BotClient.Instance.LastButtonIdClickedForEmbeded.Add(message.Id, ButtonType.Unknown);
            if (!ButtonDeleteTimers.ContainsKey(message.Id))
                ButtonDeleteTimers.Add(message.Id, new Timer(
                    async _ => await DeleteButtons(message)
                ));
            ResetTimer(message);
        }

        public static void PauseTimer(IUserMessage message)
        {
            if (ButtonDeleteTimers.TryGetValue(message.Id, out Timer? timer) && timer != null){
                timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public static void ResetTimer(IUserMessage message)
        {
            if (ButtonDeleteTimers.TryGetValue(message.Id, out Timer? timer) && timer != null){
                timer.Change(TimeSpan.FromSeconds(BUTTON_DELETE_TIME), TimeSpan.FromSeconds(BUTTON_DELETE_TIME));
            }
        }

        private async static Task DeleteButtons(IUserMessage message)
        {
            await message.ModifyAsync(
                msg => msg.Components = new ComponentBuilder().Build(), 
                options: new RequestOptions
                {
                    Timeout = 5000,
                    RetryMode = RetryMode.RetryRatelimit | RetryMode.RetryTimeouts,
                }
            );

            BotClient.Instance.CurrentPageForEmbed.Remove(message.Id);
            BotClient.Instance.LastButtonIdClickedForEmbeded.Remove(message.Id);
            ActiveEmbeds.Remove(message.Id);
            ButtonDeleteTimers[message.Id].Dispose();
            ButtonDeleteTimers.Remove(message.Id); 
        }

        public static Embed GetEmbedForBeatmapPlaycount
            (int pageNumber, List<BeatmapPlaycount> beatmapPlaycount, User user, PlaycountEmbedContext ctx, UserRankStatistic? stats)
        {
            if (beatmapPlaycount.Count == 1) return GetEmbedForSingleBeatmapPlaycount(beatmapPlaycount[0], user, stats);
            var embed = new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithName($"{user.Username}: {stats?.Pp ?? 0}PP (#{stats?.Rank ?? 0}, {user.CountryCode}{stats?.CountryRank ?? 0})")
                    .WithUrl($"https://osu.ppy.sh/users/{user.Id}")
                    .WithIconUrl($"https://flagcdn.com/w40/{user.CountryCode.ToLower()}.png")
                )
                .WithThumbnailUrl(user.AvatarUrl)
                .WithTitle("Most played maps:")
                .WithDescription(string.Join("\n", beatmapPlaycount.Select(x => x.ToString())))
                .WithFooter($"Page {pageNumber}/{ctx.TotalPages}\nFound {ctx.ResultCount} Results")
                .WithColor(Color.Green)
                .Build();

            return embed;
        }

        public static Embed GetEmbedForSingleBeatmapPlaycount(BeatmapPlaycount beatmapPlaycount, User user, UserRankStatistic? stats)
        {
            return new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithName($"{user.Username}: {stats?.Pp ?? 0}PP (#{stats?.Rank ?? 0}, {user.CountryCode}{stats?.CountryRank ?? 0})")
                    .WithUrl($"https://osu.ppy.sh/users/{user.Id}")
                    .WithIconUrl($"https://flagcdn.com/w40/{user.CountryCode.ToLower()}.png")
                )
                .WithImageUrl(beatmapPlaycount.BeatmapSet.Covers.SlimCover2x)
                .WithTitle($"{user.Username} has played this map {beatmapPlaycount.Count} times")
                .WithDescription($"[{beatmapPlaycount.BeatmapSet.Artist} - {beatmapPlaycount.BeatmapSet.Title} [{beatmapPlaycount.Beatmap.Version}]](https://osu.ppy.sh/beatmaps/{beatmapPlaycount.BeatmapId})")
                .WithColor(Color.Green)
                .Build();
        }

        public static Embed BuildErrorEmbed(string message)
            => new EmbedBuilder()
                .WithDescription($"{message}")
                .WithColor(Color.Red)
                .Build();

        public static Embed BuildSuccessEmbed(string message)
            => GetSuccessEmbedBaseBuilder(message).Build();

        public static Embed BuildSuccessEmbed(string message, string imageUrl)
            => GetSuccessEmbedBaseBuilder(message)
                .WithThumbnailUrl(imageUrl)
                .Build();
                
        public static Embed BuildSuccessEmbed(string title, string imageUrl, string description)
            => GetSuccessEmbedBaseBuilder(title)
                .WithThumbnailUrl(imageUrl)
                .WithDescription(description)
                .Build();

        public static EmbedBuilder GetSuccessEmbedBaseBuilder(string message)
            => new EmbedBuilder()
                .WithTitle($"**{message}**")
                .WithColor(Color.Green);

        public static Embed BuildInfoEmbed(string message)
            => new EmbedBuilder()
                .WithDescription(message)
                .WithColor(Color.Blue)
                .Build();

        public static async Task PageForEmbedUpdated(
            IOsuWebClient osuWebClient, IBeatmapRepository beatmapRepo, ulong id)
        {
            if (ActiveEmbeds.ContainsKey(id)){
                PlaycountEmbedContext context = ActiveEmbeds[id];
                int pageNumber = BotClient.Instance.CurrentPageForEmbed[context.Message.Id];
                List<BeatmapPlaycount> mostPlayed = await QueryPlaycountBasedOffEmbedContext(
                    osuWebClient, beatmapRepo, context, pageNumber
                );
                var stats = await osuWebClient.GetUserRankStatistics(context.User.Id);
                var newEmbed = GetEmbedForBeatmapPlaycount(pageNumber, mostPlayed, context.User, context, stats);
                await context.Message.ModifyAsync(
                    msg => { msg.Embed = newEmbed; msg.Components = Button.GetPageButtonGroup(id); },
                    options: new RequestOptions
                    {
                        Timeout = 5000,
                        RetryMode = RetryMode.RetryTimeouts,
                    }
                );
            }
        }

        public static Embed BuildHelpEmbed()
            => new EmbedBuilder()
                .WithColor(Color.Teal)
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithName("Command list for ospc:")
                    .WithIconUrl("https://i.imgur.com/7Gu9beA.png")
                )
                .WithDescription($"**Settings** - {ConvertToInline("load", "link-profile")}\n**Osu** - {ConvertToInline("most-played", "playcount", "search")}")
                .Build();
        
        public static Embed BuildHelpEmbedForCommand(string command)
        {
            switch (command){
                case "load": return BuildLoadCommandHelpEmbed();
                case "link-profile": return BuildLinkProfileCommandHelpEmbed();
                case "most-played": case "mp": return BuildMostPlayedCommandHelpEmbed();
                case "playcount": case "pc": return BuildPlaycountCommandHelpEmbed();
                case "search": return BuildSearchCommandHelpEmbed();
                default: return BuildErrorEmbed($"Command: {command} doesn't exist!");
            }
        }

        private static Embed BuildLoadCommandHelpEmbed()
            => new EmbedBuilder()
                .WithColor(Color.Teal)
                .WithTitle("load")
                .WithDescription(
                    @"Load or update your saved beatmap playcounts. 
                    
                    Use this command if the data for your beatmap playcounts isn't loaded or is inaccurate.
                    
                    **Format**
                    `=load [username]`

                    **Parameters**
                    `username`: osu username of the player to load data for. Required if you haven't linked your discord profile to an osu account.
                    ")
                .Build();

        private static Embed BuildLinkProfileCommandHelpEmbed()
            => new EmbedBuilder()
                .WithColor(Color.Teal)
                .WithTitle("link-profile")
                .WithDescription(
                    @"Link your discord account to an osu profile. 
                    
                    **Format**
                    =link [username]

                    **Parameters**
                    `username`: osu username of the player to link. 
                    ")
                .Build();

        private static Embed BuildMostPlayedCommandHelpEmbed()
             => new EmbedBuilder()
                .WithColor(Color.Teal)
                .WithTitle("most-played")
                .WithDescription(
                    @"Display a user's most played beatmaps. 
                    
                    **Format**
                    =most-played [username]

                    **Parameters**
                    `username`: osu username of the player to display most played beatmaps. Required if you haven't linked your discord profile to an osu account.

                    aliases: `mp`
                    ")
                .Build();

        private static Embed BuildPlaycountCommandHelpEmbed()
            => new EmbedBuilder()
                .WithColor(Color.Teal)
                .WithTitle("playcount")
                .WithDescription(
                    @"Display the amount of times a user submitted a play on a map. 
                    
                    **Format**
                    =playcount [username] [map url / map id]

                    **Parameters**
                    `username`: osu username of the player to display most played beatmaps. Required if you haven't linked your discord profile to an osu account.
                    `map url`: url of beatmap difficulty to check playcount
                    `map url`: url of beatmap id to check playcount

                    When no argument is provided for map url / map id, beatmap playcount is shown for the link of the map last sent in the channel

                    **Examples of use**
                    `=playcount`
                    `=playcount https://osu.ppy.sh/beatmapsets/396221#osu/924759`
                    `=playcount 924759`
                    `=playcount peppy https://osu.ppy.sh/beatmapsets/396221#osu/924759`
                    `=playcount peppy 924759`

                    aliases: `pc`
                    ")
                .Build();

        private static Embed BuildSearchCommandHelpEmbed()
            => new EmbedBuilder()
                .WithColor(Color.Teal)
                .WithTitle("search")
                .WithDescription(
                    @"Filter user's beatmap playcounts based on queries 
                    
                    **Format**
                    =search [username] [flag] [filter]

                    You can chain multiple flags and queries together for a more refined search
                    
                    **Parameters**
                    `username`: osu username of the player to display most played beatmaps. Required if you haven't linked your discord profile to an osu account.
                    `flag`: Type of filter
                    `filter`: Filter details

                    **Flags**
                    `-q [query]`: Return results where [query] is contained in beatmap artist or title. Ignored if `-a` or `-t` flag is used
                    `-a [artist]`: Return results where [artist] is contained in beatmap artist.
                    `-t [title]`: Return results where [title] is contained in beatmap title.
                    `-e`: Return results if query is an exact match (not case sensitive) rather than if it is only contained. Use with `-q`, `-a`, or `-t` flag.
                    `playcount(<|<=|=|>=|>)[count]`: Return results where beatmap playcount matches the comparison. Aliases: `pc`, `count`, `c`
                    `[attribute](<|<=|=|>=|>)[count]`: Return results where specified beatmap attribute [attribute] matches the comparison.

                    **Filterable beatmap attributes**: 
                    `cs - Circle size`
                    `bpm - Beats per minute`
                    `length`
                    `hp drain`
                    `od - Overall difficulty`
                    `ar - Approach rate`
                    `sr - Star rating`

                    When filtering beatmap attributes, you can compare if an attribute if greater, less or equal to, or you can use two comparisons to filter attributes if they are in between two values

                    **Examples of use**
                    `=search opensand -q feryquitous`
                    `=search opensand -q an -e`
                    `=search opensand -t tsuki`
                    `=search opensand -t saigetsu`
                    `=search opensand -t saigetsu -e`
                    `=search opensand c>100`
                    `=search opensand c=4`
                    `=search opensand c>=3.5 c<=3.8`
                    `=search opensand sr>7`

                    **Stuff that should work but doesn't yet (hopefully will add later xd)**
                    `=search opensand -a 'MY FIRST STORY'` (I forgot to allow searching for strings with spaces for queries)
                    `=search opensand c>100 c<200` (Can't search for maps with playcounts in between numbers yet)
                    ")
                .Build();

        private static string ConvertToInline(params string[] words) 
            => words.Aggregate("", (acc, curr) => acc += $"`{curr}` ");

        private static async Task<List<BeatmapPlaycount>> QueryPlaycountBasedOffEmbedContext(
            IOsuWebClient osuWebClient,
            IBeatmapRepository beatmapRepo,
            PlaycountEmbedContext context, 
            int pageNumber) 
        {
            if (!context.Filtered) {
                return await osuWebClient.GetBeatmapPlaycountsForUser
                (context.User.Id, LIMIT, LIMIT * (pageNumber-1));
            } else {
                return await beatmapRepo.FilterBeatmapPlaycountsForUser(
                    context.SearchParams,
                    context.User.Id, 
                    LIMIT, 
                    pageNumber
                );
            }
        }
    }
}
