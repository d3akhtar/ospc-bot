
using Discord;

using OSPC.Bot.Component;
using OSPC.Domain.Common;

namespace OSPC.Bot.Command.Result
{
    public class Reply
    {
        public required Embed Embed { get; set; }
        public bool Successful { get; set; }

        public static Reply Error(string message)
            => new()
            {
                Embed = EmbededUtils.BuildErrorEmbed(message),
                Successful = false
            };

        public static Reply Error(Error error)
            => Error(error.Message!);

        public static Reply Success(Embed embed)
            => new()
            {
                Embed = embed,
                Successful = true
            };

        public override string ToString()
            => $"Success: {Successful} EmbedContent: {Embed.Description}";
    }
}