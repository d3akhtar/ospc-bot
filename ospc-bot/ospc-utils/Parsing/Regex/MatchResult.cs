using OSPC.Domain.Common;
using OSPC.Utils.Parsing.RegularExpressions.NamedGroupMatchValues;

namespace OSPC.Utils.Parsing.RegularExpressions
{
    public class MatchResult
    {
        public Dictionary<string, NamedGroupMatchValue> Matches { get; set; } = new();
        public string LeftoverInput { get; set; } = string.Empty;
        public bool IsError { get; set; } = false;
        public string? ErrorMessage { get; set; } = null;

        public bool FullMatch => LeftoverInput.Length == 0;

        public static MatchResult Error(string message)
            => new()
            {
                IsError = true,
                ErrorMessage = message
            };

        public static MatchResult Error(Error error)
            => new()
            {
                IsError = true,
                ErrorMessage = error.Message
            };

        public static MatchResult Success(List<NamedGroupMatchValue> matchedValues, string cleaned)
        {
            MatchResult res = new();
            foreach (var value in matchedValues)
                res.Matches.Add(value.Name, value);
            res.LeftoverInput = cleaned.Trim();

            return res;
        }
    }
}