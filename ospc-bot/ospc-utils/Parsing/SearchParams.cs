using System.Linq.Expressions;
using OSPC.Utils.RegularExpressions;

namespace OSPC.Utils
{
    public class SearchParams
    {
        private static ParameterExpression _sParam = Expression.Parameter(typeof(SearchParams), "s");
        private static Dictionary<string, ISetter<SearchParams>> _regexGroupBindingInfo = new();
        
        public required string Username { get; set; }
        public bool Exact { get; set; }
        public string Query { get; set; } = "";
        public string? Artist { get; set; }
        public string? Title { get; set; }
        public ComparisonFilter? Playcount { get; set; }
        public BeatmapFilter? BeatmapFilter { get; set; }

        public static void ConfigureBindings()
        {
            RegexEvaluator rb = new();
            rb.AddFlag(name: GetMemberName(s => s.Username), RegexFlagMatchOptions.Word | RegexFlagMatchOptions.Numbers | RegexFlagMatchOptions.Multi, []);
            rb.AddFlag(name: GetMemberName(s => s.Exact), RegexFlagMatchOptions.NoInput, ["-e", "--exact"]);
            rb.AddFlag(name: GetMemberName(s => s.Query), RegexFlagMatchOptions.Word | RegexFlagMatchOptions.Numbers | RegexFlagMatchOptions.Multi, "-q", "--query");
            rb.AddFlag(name: GetMemberName(s => s.Artist), RegexFlagMatchOptions.Word | RegexFlagMatchOptions.Numbers | RegexFlagMatchOptions.Multi, "-a", "--artist");
            rb.AddFlag(name: GetMemberName(s => s.Title), RegexFlagMatchOptions.Word | RegexFlagMatchOptions.Numbers | RegexFlagMatchOptions.Multi, "-t", "--title");
            
            BindPropertyToRegexGroup(s => s.Username);
            BindPropertyToRegexGroup(s => s.Exact);
            BindPropertyToRegexGroup(s => s.Query);
            BindPropertyToRegexGroup(s => s.Artist);
            BindPropertyToRegexGroup(s => s.Title);
        }

        public static void BindPropertyToRegexGroup<T>(Expression<Func<SearchParams,T>> exp)
        {
            MemberExpression me = exp.Body switch
        	{
        		UnaryExpression u => (u.Operand as MemberExpression)!,
        		MemberExpression m => m!,
        		_ => throw new Exception()
        	};

            var valueParam = Expression.Parameter(me.Type, "value");
            var instanceParam = exp.Parameters[0];

            var assign = Expression.Assign(me, valueParam);
            var lambda = Expression.Lambda<Action<SearchParams, T>>(assign, instanceParam, valueParam);

            var action = lambda.Compile();

            var setter = new Setter<SearchParams, T>(action);

            _regexGroupBindingInfo.Add(me.Member.Name, setter);
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

        public static SearchParams? GetSearchParamsFromInput(string input)
        {
            (bool success, BeatmapFilter? beatmapFilter, input) = BeatmapFilter.ParseBeatmapFilter(input);
            if (!success) return null;
            
            (success, string query, string artist, string title, bool exact, input) = ProcessFlags(input);
            if (!success) return null;
            
            (success, ComparisonFilter count, input) = GetPlaycountComparison(input);
            if (!success) return null;
            
            (success, string username, input) = GetUsername(input);
            if (!success) return null;
            
            return new SearchParams {
                Username = username.Trim('\"'),
                Exact = exact,
                Query = query.Trim('\"'),
                Artist = artist.Trim('\"'),
                Title = title.Trim('\"'),
                Playcount = count,
                BeatmapFilter = beatmapFilter
            };
        }
        
        private static (bool success, string username, string cleaned) GetUsername(string input)
        {
            string username = "", cleaned = input;
            var matches = RegexPatterns.UsernameRegex.Matches(input);
            if (matches.Count > 1) return (false, username, cleaned);
            else if (matches.Count == 1) {
                username = matches[0].Groups[0].Value;
                cleaned = RegexPatterns.UsernameRegex.Replace(input, "");
            }
            return (true, username, cleaned);
        }
        
        private static (bool success, string query, string artist, string title, bool exact, string cleaned) ProcessFlags(string input)
        {
            string query = "", artist = "", title = "", cleaned = input; bool exact = false;
            
            var matches = RegexPatterns.InputFlagRegex.Matches(input);
            for (int i = 0; i < matches.Count; i++) {
                var match = matches[i];
                var flag = match.Groups[1].Value;
                var val = match.Groups[2].Value;
                switch (flag) {
                    case "a": 
                        if (!string.IsNullOrEmpty(artist)) return (false, query, artist, title, exact, cleaned);
                        else artist = val; 
                        break;
                    case "t": 
                        if (!string.IsNullOrEmpty(title)) return (false, query, artist, title, exact, cleaned);
                        else title = val; 
                        break;
                    case "q":
                        if (!string.IsNullOrEmpty(query)) return (false, query, artist, title, exact, cleaned);
                        else query = val; 
                        break;
                }
            }
            
            cleaned = RegexPatterns.InputFlagRegex.Replace(input, "");
            
            matches = RegexPatterns.FlagRegex.Matches(input);
            if (matches.Count > 1) return (false, query, artist, title, exact, cleaned);
            else { 
                exact = matches.Count == 1;
                cleaned = RegexPatterns.FlagRegex.Replace(cleaned, "");
            }
            
            return (true, query, artist, title, exact, cleaned);
        }
        
        private static (bool success, ComparisonFilter? count, string cleaned) GetPlaycountComparison(string input)
        {
            string cleaned = input;
            var matches = RegexPatterns.PlaycountComparisonRegex.Matches(input);
            cleaned = RegexPatterns.PlaycountComparisonRegex.Replace(input, "");
            (bool success, ComparisonFilter? filter) = ComparisonFilter.GetFilterFromMatch(matches);
            return (success, filter, cleaned);
        }

        private static string GetMemberName<T>(Expression<Func<SearchParams,T>> exp)
        {
            MemberExpression? me = exp.Body switch
            {
                   UnaryExpression u => u.Operand as MemberExpression,
                   MemberExpression m => m,
                   _ => throw new Exception()
            };

            return me?.Member.Name ?? throw new NullReferenceException("GetMemberExpressionPropertyName<TIn,TOut>()");        }
    }
}
