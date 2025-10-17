using Discord;

using OSPC.Domain.Model;
using OSPC.Parsing.ParsedObjects;

namespace OSPC.Bot.Context
{
    public class PlaycountEmbedContext
    {
        public IUserMessage? Message { get; set; }
        public required User User { get; set; }
        public bool Filtered { get; set; }
        public bool General { get; set; }
        public SearchParams? SearchParams { get; set; }
        public int ResultCount { get; set; }
        public int TotalPages { get; set; }
    }
}