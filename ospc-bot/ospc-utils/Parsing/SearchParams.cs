using User =  OSPC.Domain.Model.User;
using OSPC.Utils.Parsing.RegularExpressions.Limitations;
using OSPC.Utils.Parsing.RegularExpressions;
using OSPC.Utils.Parsing.RegularExpressions.NamedGroupMatchValues;
using OSPC.Utils.Parsing.RegularExpressions.Results;

namespace OSPC.Utils.Parsing
{
    public class SearchParams : Parsable<SearchParams>
    {
        public string Username { get; set; } = User.Unspecified;
        public bool Exact { get; set; } = false;
        public string Query { get; set; } = "";
        public string? Artist { get; set; }
        public string? Title { get; set; }
        public ComparisonFilter? Playcount { get; set; }
        public BeatmapFilter? BeatmapFilter { get; set; }

        static SearchParams()
        {
            SetupEvaluator();
        }
        
        public static SearchParams Empty = new()
        {
            Artist = string.Empty,
            Title = string.Empty,
            BeatmapFilter = null,
            Exact = false,
            Playcount = null,
            Query = string.Empty,
            Username = string.Empty
        };

        public static SearchParams ForMostPlayed(string username)
            => new() {
                Username = username
            };

        public static new void SetupEvaluator()
        {
            _regexEvaluator
                .AddGroup(s => s.Username)
                    .AddLimitation(LimitMatchCount.Create(1))
                .AddFlag(s => s.Exact, RegexFlagMatchOptions.NoInput)
                    .AddLimitation(LimitMatchCount.Create(1))
                .AddFlag(s => s.Query)
                    .AddLimitation(LimitMatchCount.Create(1))
    				.AddLimitation(CannotBothExist.Create<SearchParams, string?>(s => s.Artist))
    				.AddLimitation(CannotBothExist.Create<SearchParams, string?>(s => s.Title))
                .AddFlag(s => s.Artist)
                    .AddLimitation(LimitMatchCount.Create(GetMemberName(s => s.Artist), 1))
    				.AddLimitation(CannotBothExist.Create(GetMemberName(s => s.Artist), GetMemberName(s => s.Query)))
                .AddFlag(s => s.Title)
                    .AddLimitation(LimitMatchCount.Create(1))
    				.AddLimitation(CannotBothExist.Create<SearchParams, string?>(s => s.Query))
                .AddNumberComparison(
                        s => s.Playcount,
                        RegexFlagMatchOptions.StrictlyNumbers | 
				        RegexFlagMatchOptions.Positive | 
				        RegexFlagMatchOptions.WholeNumbers,
                        "c", "pc", "playcount", "count"
                    )
    				.AddLimitation(LimitMatchCount.Create(2));
                             

            BindSetterToRegexGroup(s => s.Username, (instance, matchValue)
                => ReturnResultBasedOnMatchType<StringValue>(instance, (instance, matchValue) => instance.Username = matchValue.Value, matchValue));
                
            BindSetterToRegexGroup(s => s.Exact, (instance, matchValue)
                => ReturnResultBasedOnMatchType<StringValue>(instance, (instance, matchValue) => instance.Exact = true, matchValue));

            BindSetterToRegexGroup(s => s.Query, (instance, matchValue)
                => ReturnResultBasedOnMatchType<StringValue>(instance, (instance, matchValue) => instance.Query = matchValue.Value, matchValue));
                
            BindSetterToRegexGroup(s => s.Artist, (instance, matchValue)
                => ReturnResultBasedOnMatchType<StringValue>(instance, (instance, matchValue) => instance.Artist = matchValue.Value, matchValue));            
            
            BindSetterToRegexGroup(s => s.Title, (instance, matchValue)
                => ReturnResultBasedOnMatchType<StringValue>(instance, (instance, matchValue) => instance.Title = matchValue.Value, matchValue));            
            
            BindSetterToRegexGroup(s => s.Playcount, (instance, matchValue)
                => ReturnResultBasedOnMatchType<NumberComparisonValue>(instance, (instance, matchValue) => instance.Playcount = ComparisonFilter.Create(matchValue), matchValue));            
            
            BindParsablePropertySetters<BeatmapFilter>((instance, input) => {
                ParseResult<BeatmapFilter> result = BeatmapFilter.Parse(input);
                if (result.Successful) instance.BeatmapFilter = result.Value;
                return result;
            });
        }

        
    }
}
