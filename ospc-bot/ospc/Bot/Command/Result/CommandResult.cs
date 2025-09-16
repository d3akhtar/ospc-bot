
using Discord;

using OSPC.Bot.Component;
using OSPC.Utils;

namespace OSPC.Bot.Command.Result
{
    public class CommandResult
    {
        public required Embed Embed { get; set; }
        public bool Successful { get; set; }

        public static CommandResult Error(string message)
            => new()
            {
                Embed = Embeded.BuildErrorEmbed(message),
                Successful = false
            };

        public static CommandResult Error(Error error)
            => Error(error.Message!);

        public static CommandResult Success(Embed embed)
            => new()
            {
                Embed = embed,
                Successful = true
            };

        public override string ToString()
            => $"Success: {Successful} EmbedContent: {Embed.Description}";
    }
}