using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace OSPC.Utils.Parsing
{
    public class BeatmapFilter
    {
        public ComparisonFilter? CircleSize { get; set; }
        public ComparisonFilter? BPM { get; set; }
        public ComparisonFilter? Length { get; set; }
        public ComparisonFilter? HpDrain { get; set; }
        public ComparisonFilter? OD { get; set; }
        public ComparisonFilter? AR { get; set; }
        public ComparisonFilter? DifficultyRating { get; set; }

        public static string GetFilterAbbreviation(Expression<Func<BeatmapFilter,ComparisonFilter>> exp)
        => (exp.Body as MemberExpression)!.Member.Name switch {
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

        public static (bool success, BeatmapFilter? filter, string cleaned) ParseBeatmapFilter(string input)
        {
            string cleaned = input;
            
            (bool success, ComparisonFilter? cs, cleaned) = GetBeatmapAttributeFilter(b => b.CircleSize!, cleaned);
            if (!success) return (false, null, cleaned);
            (success, ComparisonFilter? bpm, cleaned) = GetBeatmapAttributeFilter(b => b.BPM!, cleaned);
            if (!success) return (false, null, cleaned);
            (success, ComparisonFilter? length, cleaned) = GetBeatmapAttributeFilter(b => b.Length!, cleaned);
            if (!success) return (false, null, cleaned);
            (success, ComparisonFilter? hpDrain, cleaned) = GetBeatmapAttributeFilter(b => b.HpDrain!, cleaned);
            if (!success) return (false, null, cleaned);
            (success, ComparisonFilter? od, cleaned) = GetBeatmapAttributeFilter(b => b.OD!, cleaned);
            if (!success) return (false, null, cleaned);
            (success, ComparisonFilter? ar, cleaned) = GetBeatmapAttributeFilter(b => b.AR!, cleaned);
            if (!success) return (false, null, cleaned);
            (success, ComparisonFilter? difficultyRating, cleaned) = GetBeatmapAttributeFilter(b => b.DifficultyRating!, cleaned);
            if (!success) return (false, null, cleaned);
            
            BeatmapFilter filter = new()
            {
                CircleSize = cs,
                BPM = bpm,
                Length = length,
                HpDrain = hpDrain,
                OD = od,
                AR = ar,
                DifficultyRating = difficultyRating
            };

            Console.WriteLine(cleaned);
            
            return (success, filter, cleaned);
        }

        private static string GetClauseForAttribute(BeatmapFilter beatmapFilter, Expression<Func<BeatmapFilter,ComparisonFilter>> exp)
            => ComparisonConverter.CreateComparisonClause(
                exp.Compile().Invoke(beatmapFilter), "Beatmaps", 
                (exp.Body as MemberExpression)!.Member.Name
            );
        
        private static (bool success, ComparisonFilter? filter, string cleaned) GetBeatmapAttributeFilter(Expression<Func<BeatmapFilter,ComparisonFilter>> exp, string input)
        {
            string abbreviation = GetFilterAbbreviation(exp), cleaned = input;
            Regex filterRegex = new Regex(RegexPatterns.BeatmapFilterRegexTemplate.Replace("{abbreviation}", abbreviation));
            var matches = filterRegex.Matches(input);
            cleaned = filterRegex.Replace(input, "");
            (bool success, ComparisonFilter? filter) = ComparisonFilter.GetFilterFromMatch(matches);
            return (success, filter, cleaned);
        }

        public override string ToString()
            => GetClause();
    }
}
