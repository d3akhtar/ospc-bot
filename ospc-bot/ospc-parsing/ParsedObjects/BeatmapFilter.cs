using System.Linq.Expressions;
using OSPC.Parsing.Converters;
using OSPC.Parsing.Filters;
using OSPC.Parsing.Regex.Limitations;
using OSPC.Parsing.Regex.NamedGroupMatchValues;

namespace OSPC.Parsing.ParsedObjects
{
    public class BeatmapFilter : Parsable<BeatmapFilter>
    {
        public ComparisonFilter? CircleSize { get; set; }
        public ComparisonFilter? BPM { get; set; }
        public ComparisonFilter? Length { get; set; }
        public ComparisonFilter? HpDrain { get; set; }
        public ComparisonFilter? OD { get; set; }
        public ComparisonFilter? AR { get; set; }
        public ComparisonFilter? DifficultyRating { get; set; }

        static BeatmapFilter()
        {
            SetupEvaluator();
        }

        public static string GetFilterAbbreviation(Expression<Func<BeatmapFilter, ComparisonFilter>> exp)
        => (exp.Body as MemberExpression)!.Member.Name switch
        {
            "CircleSize" => "cs",
            "BPM" => "bpm",
            "Length" => "length",
            "HpDrain" => "drain",
            "OD" => "od",
            "AR" => "ar",
            "DifficultyRating" => "sr",
            _ => "?"
        };

        public string GetClause()
            => GetClauseForAttribute(this, b => b.CircleSize!) +
               GetClauseForAttribute(this, b => b.BPM!) +
               GetClauseForAttribute(this, b => b.Length!) +
               GetClauseForAttribute(this, b => b.HpDrain!) +
               GetClauseForAttribute(this, b => b.OD!) +
               GetClauseForAttribute(this, b => b.AR!) +
               GetClauseForAttribute(this, b => b.DifficultyRating!);

        private static string GetClauseForAttribute(BeatmapFilter beatmapFilter, Expression<Func<BeatmapFilter, ComparisonFilter>> exp)
            => ComparisonConverter.CreateComparisonClause(
                exp.Compile().Invoke(beatmapFilter), "Beatmaps",
                (exp.Body as MemberExpression)!.Member.Name
            );

        public static new void SetupEvaluator()
        {
            _regexEvaluator
                .AddNumberComparison(b => b.CircleSize, GetFilterAbbreviation(b => b.CircleSize!))
                    .AddLimitation(LimitMatchCount.Create(2))
                .AddNumberComparison(b => b.BPM, GetFilterAbbreviation(b => b.BPM!))
                    .AddLimitation(LimitMatchCount.Create(2))
                .AddNumberComparison(b => b.Length, GetFilterAbbreviation(b => b.Length!))
                    .AddLimitation(LimitMatchCount.Create(2))
                .AddNumberComparison(b => b.HpDrain, GetFilterAbbreviation(b => b.HpDrain!))
                    .AddLimitation(LimitMatchCount.Create(2))
                .AddNumberComparison(b => b.OD, GetFilterAbbreviation(b => b.OD!))
                    .AddLimitation(LimitMatchCount.Create(2))
                .AddNumberComparison(b => b.AR, GetFilterAbbreviation(b => b.AR!))
                    .AddLimitation(LimitMatchCount.Create(2))
                .AddNumberComparison(b => b.DifficultyRating, GetFilterAbbreviation(b => b.DifficultyRating!))
                    .AddLimitation(LimitMatchCount.Create(2));


            BindSetterToRegexGroup(b => b.CircleSize, (instance, matchValue)
                => ReturnResultBasedOnMatchType<NumberComparisonValue>(instance, (instance, matchValue) => instance.CircleSize = ComparisonFilter.Create(matchValue), matchValue));

            BindSetterToRegexGroup(b => b.BPM, (instance, matchValue)
                => ReturnResultBasedOnMatchType<NumberComparisonValue>(instance, (instance, matchValue) => instance.BPM = ComparisonFilter.Create(matchValue), matchValue));

            BindSetterToRegexGroup(b => b.Length, (instance, matchValue)
                => ReturnResultBasedOnMatchType<NumberComparisonValue>(instance, (instance, matchValue) => instance.Length = ComparisonFilter.Create(matchValue), matchValue));

            BindSetterToRegexGroup(b => b.HpDrain, (instance, matchValue)
                => ReturnResultBasedOnMatchType<NumberComparisonValue>(instance, (instance, matchValue) => instance.HpDrain = ComparisonFilter.Create(matchValue), matchValue));

            BindSetterToRegexGroup(b => b.OD, (instance, matchValue)
                => ReturnResultBasedOnMatchType<NumberComparisonValue>(instance, (instance, matchValue) => instance.OD = ComparisonFilter.Create(matchValue), matchValue));

            BindSetterToRegexGroup(b => b.AR, (instance, matchValue)
                => ReturnResultBasedOnMatchType<NumberComparisonValue>(instance, (instance, matchValue) => instance.AR = ComparisonFilter.Create(matchValue), matchValue));

            BindSetterToRegexGroup(b => b.DifficultyRating, (instance, matchValue)
                => ReturnResultBasedOnMatchType<NumberComparisonValue>(instance, (instance, matchValue) => instance.DifficultyRating = ComparisonFilter.Create(matchValue), matchValue));
        }

        public override string ToString()
            => GetClause();
    }
}