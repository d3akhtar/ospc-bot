using Discord;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using OSPC.Bot.Command.Result;
using OSPC.Bot.Component;
using OSPC.Bot.Context;
using OSPC.Bot.Search.UserSearch;
using OSPC.Domain.Model;
using OSPC.Infrastructure.Database.Repository;
using OSPC.Infrastructure.Http;
using OSPC.Infrastructure.Job;
using OSPC.Utils;
using OSPC.Utils.Parsing;

namespace OSPC.Bot.Command
{
    public class BotCommandService : IBotCommandService
    {
        private readonly ILogger<BotCommandService> _logger;
        private readonly IUserRepository _userRepo;
        private readonly IBeatmapRepository _beatmapRepo;
        private readonly IOsuWebClient _osuWebClient;
        private readonly IPlaycountFetchJobQueue _jobQueue;
        private readonly IUserSearch _userSearch;
        private const int LIMIT = 25;

        public BotCommandService(
            ILogger<BotCommandService> logger,
            IUserRepository userRepo, IBeatmapRepository beatmapRepo, IUserSearch userSearch, 
            IOsuWebClient osuWebClient, IPlaycountFetchJobQueue jobQueue)
        {
            _logger = logger;
            _userRepo = userRepo;
            _beatmapRepo = beatmapRepo;
            _osuWebClient = osuWebClient;
            _jobQueue = jobQueue;
            _userSearch = userSearch;
        }

        public async Task<CommandResult> GetPlaycount(ChannelOsuContext ctx, string username, string beatmapLink)
        {
            int beatmapId;
            
            var match = RegexPatterns.OsuBeatmapLinkRegex.Match(beatmapLink);
            if (match.Success) beatmapId = int.Parse(match.Groups[2].Value);
            else return CommandResult.Error("Invalid beatmap link format");

            return await GetPlaycount(ctx, username, beatmapId);
        }

        public async Task<CommandResult> GetPlaycount(ChannelOsuContext ctx, string username, int beatmapId)
        {
            _logger.LogInformation("Getting playcount for {Username} on {BeatmapId} with context: {@Ctx}", username, beatmapId, ctx);
            
            try {
                if (!string.IsNullOrEmpty(username) && !RegexPatterns.StrictUsernameRegex.IsMatch(username)) 
                    return CommandResult.Error("Invalid username format!");

                User? user = await _userSearch.SearchUser(username, ctx);
                if (user == null) return CommandResult.Error("User wasn't found.");

                if (beatmapId == -1){
                    beatmapId = await _beatmapRepo.GetReferencedBeatmapIdForChannel(ctx.ChannelId) ?? -1;
                    if (beatmapId == -1) return CommandResult.Error("Beatmap wasn't found.");
                }

                BeatmapPlaycount? bpc = await _beatmapRepo.GetBeatmapPlaycountForUserOnMap(user.Id, beatmapId);
                if (bpc == null) return CommandResult.Error($"The map has either not loaded yet, or the user: `{user.Username}` hasn't played the map.");
                else {
                    UserRankStatistic? stats = await _osuWebClient.GetUserRankStatistics(user.Id);
                    return CommandResult.Success(Embeded.GetEmbedForSingleBeatmapPlaycount(bpc, user, stats));
                }
            } catch (Exception e){
                _logger.LogError(e, "An error occurred while getting playcount for {Username}, beatmapId: {BeatmapId}", username, beatmapId);
                return CommandResult.Error($"Something went wrong while getting playcount for `{username}`");
            }
        }

        public async Task<SearchResult> Search(ChannelOsuContext ctx, SearchParams searchParams)
        {
            _logger.LogInformation("Searching beatmaps with searchParams: {@SearchParams} and context; {@Ctx}", searchParams, ctx);

            User? user = await _userSearch.SearchUser(searchParams.Username, ctx);
            if (user == null) return SearchResult.Error("User wasn't found");
            try {
                List<BeatmapPlaycount> filteredMostPlayed = await _beatmapRepo.FilterBeatmapPlaycountsForUser(
                    searchParams, user.Id, LIMIT, 1
                );

                var buttons = Button.GetPageButtonGroup();
                var stats = await _osuWebClient.GetUserRankStatistics(user.Id);
                var resCount = await _beatmapRepo.GetTotalResultCountForSearch(searchParams, user.Id);
                var embedContext = new PlaycountEmbedContext{
                    User = user,
                    Filtered = true,
                    General = !string.IsNullOrEmpty(searchParams.Query),
                    SearchParams = searchParams,
                    ResultCount = resCount,
                    TotalPages = (resCount / LIMIT) + 1
                };
                var embed = Embeded.GetEmbedForBeatmapPlaycount(1, filteredMostPlayed, user, embedContext, stats);
                return new SearchResult {
                    Embed = embed,
                    Components = buttons,
                    Context = embedContext,
                    Successful = true
                };
            } catch (Exception e) {
                _logger.LogError(e, "An error occurred while executing search command with params: {@SearchParams}", searchParams);
                return SearchResult.Error($"Something went wrong while searching beatmap playcounts for {user!.Username}");
            }
        }

        public async Task<SearchResult> GetMostPlayed(ChannelOsuContext ctx, string username)
            => await Search(ctx, SearchParams.ForMostPlayed(username));   

        public async Task<CommandResult> LinkProfile(ChannelOsuContext ctx, string username)
        {
           _logger.LogInformation("Linking profile for {Username} using context: {@Ctx}", username, ctx);
             
            try {
                User? user = await _userSearch.SearchUser(username, ctx);
                if (user == null) return CommandResult.Error("User not found!");
            
                if (!await _userRepo.AddDiscordPlayerMapping(ctx.DiscordUserId, user.Id))
                    return CommandResult.Error($"Something went wrong while mapping user {username}");
                else {
                    string message = $"User: \'{username}\' successfully mapped.";
                    string description = $"Any commands you make with an unspecified username will get stats for [{user.Username}]({user.ProfileUrl})";

                    return CommandResult.Success(Embeded.BuildSuccessEmbed(message, user.AvatarUrl, description));
                }
                
            } catch (Exception e) {
                _logger.LogError(e, "An error occured while linking {Username} using channel context: {@ChannelOsuContext}", username, ctx);
                return CommandResult.Error($"Something went wrong while linking user {username}");
            }
        }

        public async Task<CommandResult> LoadBeatmapPlaycounts(ChannelOsuContext ctx, string username)
        {
            _logger.LogInformation("Loading beatmap playcounts for {Username} using context: {Ctx}", username, ctx);

            try {
                if (!RegexPatterns.StrictUsernameRegex.IsMatch(username)) return CommandResult.Error("Invalid username format!");
                User? user = await _userSearch.SearchUser(username, ctx);
                if (user == null) return CommandResult.Error("User not found!");
                if (_jobQueue.MapsBeingFetched(user.Id)) {
                    var usersInQueue = _jobQueue.GetQueuedUsers().Aggregate("", (acc, curr) => acc += curr + "\n");
                    return CommandResult.Error($"Maps are currently being fetched. Users in queue: \n\n {usersInQueue}");
                } else {
                    await _jobQueue.EnqueueAsync(user.Id, user.Username);
                    return CommandResult.Success(Embeded.BuildSuccessEmbed($"{user.Username} has been added to the queue"));
                }
            } catch (Exception e) {
                _logger.LogError(e, "An error occurred while loading beatmap playcounts for {Username} using context: {ChannelOsuContext}", username, ctx);
                return CommandResult.Error($"Something went wrong while loading beatmap playcounts for {username}");
            }
        }
    }
}
