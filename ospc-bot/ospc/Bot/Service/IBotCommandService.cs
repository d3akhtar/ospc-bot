using OSPC.Bot.Command.Result;
using OSPC.Bot.Context;
using OSPC.Parsing.ParsedObjects;

namespace OSPC.Bot.Service
{
    public interface IBotCommandService
    {
        public Task<Reply> GetPlaycount(ChannelOsuContext ctx, string username, int beatmapId);
        public Task<Reply> GetPlaycount(ChannelOsuContext ctx, string username, string beatmapLink);
        public Task<PagedReply> Search(ChannelOsuContext ctx, SearchParams searchParams);
        public Task<PagedReply> GetMostPlayed(ChannelOsuContext ctx, string username);
        public Task<Reply> LoadBeatmapPlaycounts(ChannelOsuContext ctx, string username);
        public Task<Reply> LinkProfile(ChannelOsuContext ctx, string username);
    }
}
