namespace OSPC.Utils
{
    public enum Comparison
    {
        None,
        Less,
        LessOrEqual,
        Equal,
        GreaterOrEqual,
        Greater
    }

    public static class ComparisonConverter
    {
        public static string ConvertToString(Comparison comparison)
            => comparison switch {
                Comparison.None => "",
                Comparison.Less => "<",
                Comparison.LessOrEqual => "<=",
                Comparison.Equal => "=",
                Comparison.Greater => ">",
                Comparison.GreaterOrEqual => ">=",
                _ => ""
            };

        public static string CreateComparisonClause(ComparisonFilter filter, string table, string attribute)
        {
            if (filter == null) return "";
            else if (filter is SingleFilter s)
                return $"AND {table}.{attribute}{CreateExpressionForSingleFilter(s)} ";
            else if (filter is BetweenFilter b)
                return $@"
                            AND {table}.{attribute}{CreateExpressionForSingleFilter(b.Min)} 
                            AND {table}.{attribute}{CreateExpressionForSingleFilter(b.Max)}
                        ";
            else return "";
        }

        private static string CreateExpressionForSingleFilter(SingleFilter s)
            => $"{ConvertToString(s.Comparison)}{s.Value}";

        public static Comparison Convert(string input)
            => input switch {
                "<" => Comparison.Less,
                "<=" => Comparison.LessOrEqual,
                "=" => Comparison.Equal,
                ">" => Comparison.Greater,
                ">=" => Comparison.GreaterOrEqual,
                _ => Comparison.None
            };
    }
}
