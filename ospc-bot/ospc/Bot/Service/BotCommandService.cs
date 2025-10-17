using Microsoft.Extensions.Logging;
using OSPC.Bot.Command.Result;
using OSPC.Bot.Component;
using OSPC.Bot.Context;
using OSPC.Domain.Model;
using OSPC.Infrastructure.Database.Repository;
using OSPC.Infrastructure.Http;
using OSPC.Infrastructure.Job;
using OSPC.Utils;
using OSPC.Parsing.ParsedObjects;
using OSPC.Domain.Constants;

namespace OSPC.Bot.Service
{
    public class BotCommandService : IBotCommandService
    {
        private readonly ILogger<BotCommandService> _logger;
        private readonly IUserRepository _userRepo;
        private readonly IBeatmapRepository _beatmapRepo;
        private readonly IOsuWebClient _osuWebClient;
        private readonly IPlaycountFetchJobQueue _jobQueue;
        private readonly IUserSearchService _userSearch;
        private const int LIMIT = 25;

        public BotCommandService(
            ILogger<BotCommandService> logger,
            IUserRepository userRepo, IBeatmapRepository beatmapRepo, IUserSearchService userSearch,
            IOsuWebClient osuWebClient, IPlaycountFetchJobQueue jobQueue)
        {
            _logger = logger;
            _userRepo = userRepo;
            _beatmapRepo = beatmapRepo;
            _osuWebClient = osuWebClient;
            _jobQueue = jobQueue;
            _userSearch = userSearch;
        }

        public async Task<Reply> GetPlaycount(ChannelOsuContext ctx, string username, string beatmapLink)
        {
            int beatmapId;

            var match = RegexPatterns.OsuBeatmapLinkRegex.Match(beatmapLink);
            if (match.Success)
                beatmapId = int.Parse(match.Groups[2].Value);
            else
                return Reply.Error("Invalid beatmap link format");

            return await GetPlaycount(ctx, username, beatmapId);
        }

        public async Task<Reply> GetPlaycount(ChannelOsuContext ctx, string username, int beatmapId)
        {
            _logger.LogInformation("Getting playcount for {Username} on {BeatmapId} with context: {@Ctx}", username, beatmapId, ctx);

            if (username is not Unspecified.User && !RegexPatterns.StrictUsernameRegex.IsMatch(username))
                return Reply.Error("Invalid username format!");

            var userResult = await _userSearch.SearchUser(username, ctx);
            if (!userResult.Successful)
                return Reply.Error(userResult.Error!);

            if (beatmapId is Unspecified.Beatmap)
            {
                var beatmapIdResult = await _beatmapRepo.GetReferencedBeatmapIdForChannel(ctx.ChannelId);
                if (!beatmapIdResult.Successful)
                    return Reply.Error(beatmapIdResult.Error!);
                else
                    beatmapId = (int)beatmapIdResult.Value!;
            }

            var bpcResult = await _beatmapRepo.GetBeatmapPlaycountForUserOnMap(userResult.Value!.Id, beatmapId);
            if (!bpcResult.Successful)
                return Reply.Error(bpcResult.Error!);
            else
            {
                UserRankStatistic? stats = await _osuWebClient.GetUserRankStatistics(userResult.Value.Id);
                return Reply.Success(EmbededUtils.GetEmbedForSingleBeatmapPlaycount(bpcResult.Value!, userResult.Value!, stats));
            }
        }

        public async Task<PagedReply> Search(ChannelOsuContext ctx, SearchParams searchParams)
        {
            _logger.LogInformation("Searching beatmaps with searchParams: {@SearchParams} and context; {@Ctx}", searchParams, ctx);

            var userResult = await _userSearch.SearchUser(searchParams.Username, ctx);
            if (!userResult.Successful)
                return PagedReply.Error(userResult.Error!);

            var filteredMostPlayedResult = await _beatmapRepo.FilterBeatmapPlaycountsForUser(
                searchParams, userResult.Value!.Id, LIMIT, 1
            );

            if (!filteredMostPlayedResult.Successful)
                return PagedReply.Error(filteredMostPlayedResult.Error!);

            var buttons = ButtonUtils.GetPageButtonGroup();
            var stats = await _osuWebClient.GetUserRankStatistics(userResult.Value.Id);
            var resCountResult = await _beatmapRepo.GetTotalResultCountForSearch(searchParams, userResult.Value.Id);
            if (!resCountResult.Successful)
                return PagedReply.Error(resCountResult.Error!);

            var embedContext = new PlaycountEmbedContext
            {
                User = userResult.Value,
                Filtered = true,
                General = searchParams.Query is not Unspecified.Query,
                SearchParams = searchParams,
                ResultCount = (int)resCountResult.Value!,
                TotalPages = ((int)resCountResult.Value / LIMIT) + 1
            };
            var embed = EmbededUtils.GetEmbedForBeatmapPlaycount(1, filteredMostPlayedResult.Value!, userResult.Value, embedContext, stats);

            return new PagedReply
            {
                Embed = embed,
                Components = buttons,
                Context = embedContext,
                Successful = true
            };
        }

        public async Task<PagedReply> GetMostPlayed(ChannelOsuContext ctx, string username)
            => await Search(ctx, SearchParams.ForMostPlayed(username));

        public async Task<Reply> LinkProfile(ChannelOsuContext ctx, string username)
        {
            _logger.LogInformation("Linking profile for {Username} using context: {@Ctx}", username, ctx);

            var userResult = await _userSearch.SearchUser(username, ctx);
            if (!userResult.Successful)
                return Reply.Error(userResult.Error!);

            if (!await _userRepo.AddDiscordPlayerMapping(ctx.DiscordUserId, userResult.Value!.Id))
                return Reply.Error($"Something went wrong while mapping user {username}");
            else
            {
                string message = $"User: \'{username}\' successfully mapped.";
                string description = $"Any commands you make with an unspecified username will get stats for [{userResult.Value.Username}]({userResult.Value.ProfileUrl})";

                return Reply.Success(EmbededUtils.BuildSuccessEmbed(message, userResult.Value.AvatarUrl, description));
            }
        }

        public async Task<Reply> LoadBeatmapPlaycounts(ChannelOsuContext ctx, string username)
        {
            _logger.LogInformation("Loading beatmap playcounts for {Username} using context: {Ctx}", username, ctx);

            if (!RegexPatterns.StrictUsernameRegex.IsMatch(username))
                return Reply.Error("Invalid username format!");
            var userResult = await _userSearch.SearchUser(username, ctx);
            if (!userResult.Successful)
                return Reply.Error(userResult.Error!);
            if (_jobQueue.MapsBeingFetched(userResult.Value!.Id))
            {
                var usersInQueue = _jobQueue.GetQueuedUsers().Aggregate("", (acc, curr) => acc += curr + "\n");
                return Reply.Error($"Maps are currently being fetched. Users in queue: \n\n {usersInQueue}");
            }
            else
            {
                await _jobQueue.EnqueueAsync(userResult.Value.Id, userResult.Value.Username);
                return Reply.Success(EmbededUtils.BuildSuccessEmbed($"{userResult.Value.Username} has been added to the queue"));
            }
        }
    }
}
