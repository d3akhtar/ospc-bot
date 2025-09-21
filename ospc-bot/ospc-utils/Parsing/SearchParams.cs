using MySql.Data.MySqlClient;
using OSPC.Utils.Parsing.RegularExpressions;
using OSPC.Utils.Parsing.RegularExpressions.Limitations;
using OSPC.Utils.Parsing.RegularExpressions.NamedGroupMatchValues;
using OSPC.Utils.Parsing.RegularExpressions.Results;

using User = OSPC.Domain.Model.User;

namespace OSPC.Utils.Parsing
{
    public class SearchParams : Parsable<SearchParams>
    {
        public string Username { get; set; } = Unspecified.User;
        public bool Exact { get; set; } = false;
        public string Query { get; set; } = Unspecified.Query;
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
            => new()
            {
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

            BindParsablePropertySetters<BeatmapFilter>((instance, input) =>
            {
                ParseResult<BeatmapFilter> result = BeatmapFilter.Parse(input);
                if (result.Successful)
                    instance.BeatmapFilter = result.Value;
                return result;
            });
        }

        public MySqlCommand GetFilterQueryCommand(int userId, int pageSize, int pageNumber)
        {
            MySqlCommand cmd = new();
            cmd.CommandText = GetFilterQuery(userId, pageSize, pageNumber);
            return cmd;
        }

        public MySqlCommand GetFilterResultCountQueryCommand(int userId)
        {            
            MySqlCommand cmd = new();
            cmd.CommandText = GetFilterResultCountQuery(userId);
            return cmd;
        }

        private void SetCommandParameters(MySqlCommand command)
        {            
            if (IsGeneralQuery())
                command.Parameters.AddWithValue("@query", Query);
            else
            {
                command.Parameters.AddWithValue("@artist", Artist ?? string.Empty);
                command.Parameters.AddWithValue("@title", Title ?? string.Empty);
            }
        }
        
        private string GetFilterQuery(int userId, int pageSize, int pageNumber)
        {
            var query =
                GetQueryPartForSelect(isResultCountQuery: false) + "\n" +
                GetQueryCommonParts(userId) + "\n" +
                GetQueryPartForOrderBy(pageSize, pageNumber) + "\n";

            return query;
        }
        
        private string GetFilterResultCountQuery(int userId)
        {            
            var query =
                GetQueryPartForSelect(isResultCountQuery: true) + "\n" +
                GetQueryCommonParts(userId);

            return query;
        }

        private string GetQueryCommonParts(int userId)
        {
            var query =
                GetQueryPartForWhereClause(userId) + "\n" +
                GetQueryPartForCountComparison() + "\n" +
                GetQueryPartForArtistAndTitle() + "\n" +
                GetQueryPartForBeatmapFilter() + "\n";

            return query;
        }

        private string GetQueryPartForSelect(bool isResultCountQuery)
        {
            const string fromAndJoinClause = @"FROM BeatmapPlaycounts
                JOIN Beatmaps ON BeatmapPlaycounts.BeatmapId = Beatmaps.Id 
                JOIN BeatmapSet ON Beatmaps.BeatmapSetId = BeatmapSet.Id";

            var selectClause = isResultCountQuery ?
                "SELECT COUNT(BeatmapPlaycounts.Count)":
                "SELECT BeatmapPlaycounts.*,Beatmaps.Id,Beatmaps.Version,Beatmaps.DifficultyRating,Beatmaps.BeatmapSetId,BeatmapSet.*";

            return
                selectClause + "\n" +
                fromAndJoinClause;
        }

        private string GetQueryPartForWhereClause(int userId)
            => $"WHERE BeatmapPlaycounts.UserId = {userId}";
        
        private string GetQueryPartForCountComparison()
            => ComparisonConverter.CreateComparisonClause(Playcount!, "BeatmapPlaycounts", "Count");
            
        private string GetQueryPartForArtistAndTitle()
        {
            bool generalQuery = IsGeneralQuery();
            string artistConcat = generalQuery ? (Exact && Query != "" ? "" : "%") : (Exact && Artist != "" ? "" : "%");
            string titleConcat = generalQuery ? (Exact && Query != "" ? "" : "%") : (Exact && Title != "" ? "" : "%");
            return $@"AND (
                BeatmapSet.Title LIKE CONCAT('{titleConcat}', {(generalQuery ? "@query" : "@title")}, '{titleConcat}') 
                {(generalQuery ? "OR" : "AND")} 
                BeatmapSet.Artist LIKE CONCAT('{artistConcat}', {(generalQuery ? "@query" : "@artist")}, '{artistConcat}')
            )";
        }

        private string GetQueryPartForBeatmapFilter()
            => BeatmapFilter is {} b ? b.GetClause():string.Empty;

        private string GetQueryPartForOrderBy(int pageSize, int pageNumber)
            => $@"
                ORDER BY BeatmapPlaycounts.Count DESC
                LIMIT {pageSize}
                OFFSET {(pageNumber - 1) * pageSize}
            ";

        private bool IsGeneralQuery() => Artist is Unspecified.Artist && Title is Unspecified.Title;
    }
}
