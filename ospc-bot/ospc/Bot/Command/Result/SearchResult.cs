using Discord;

using OSPC.Bot.Component;
using OSPC.Bot.Context;
using OSPC.Utils;

namespace OSPC.Bot.Command.Result
{
    public class SearchResult : CommandResult
    {
        public MessageComponent? Components { get; set; }
        public PlaycountEmbedContext? Context { get; set; }

        public new static SearchResult Error(string message)
            => new()
            {
                Embed = Embeded.BuildErrorEmbed(message),
                Successful = false
            };

        public new static SearchResult Error(Error error)
            => Error(error.Message!);
    }
}