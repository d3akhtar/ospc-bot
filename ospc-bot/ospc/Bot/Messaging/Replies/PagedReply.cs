using Discord;

using OSPC.Bot.Component;
using OSPC.Bot.Context;
using OSPC.Domain.Common;

namespace OSPC.Bot.Command.Result
{
    public class PagedReply : Reply
    {
        public MessageComponent? Components { get; set; }
        public PlaycountEmbedContext? Context { get; set; }

        public new static PagedReply Error(string message)
            => new()
            {
                Embed = EmbededUtils.BuildErrorEmbed(message),
                Successful = false
            };

        public new static PagedReply Error(Error error)
            => Error(error.Message!);
    }
}