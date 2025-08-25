using OSPC.Bot.Command.Result;
using OSPC.Utils;
using OSPC.Utils.Parsing;

namespace OSPC.Bot.Command
{
    public interface IBotCommandService
    {
        public Task<CommandResult> GetPlaycount(ChannelOsuContext ctx, string username, int beatmapId);
        public Task<CommandResult> GetPlaycount(ChannelOsuContext ctx, string username, string beatmapLink);        
        public Task<SearchResult> Search(ChannelOsuContext ctx, SearchParams searchParams);
        public Task<SearchResult> GetMostPlayed(ChannelOsuContext ctx, string username);
        public Task<CommandResult> LoadBeatmapPlaycounts(ChannelOsuContext ctx, string username);
        public Task<CommandResult> LinkProfile(ChannelOsuContext ctx, string username);
    }
}
