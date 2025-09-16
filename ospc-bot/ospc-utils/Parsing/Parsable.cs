using System.Linq.Expressions;
using OSPC.Utils.Helpers;
using OSPC.Utils.Parsing.RegularExpressions;
using OSPC.Utils.Parsing.RegularExpressions.NamedGroupMatchValues;
using OSPC.Utils.Parsing.RegularExpressions.Results;

namespace OSPC.Utils.Parsing
{
	public class Parsable<T> where T : class, new()
	{
		protected static RegexEvaluator<T> _regexEvaluator = new();

        protected static Dictionary<string, Func<T, NamedGroupMatchValue, Result>> _setters = new();
        protected static List<Func<T, string, ParseResult>> _parsableSetters = new();

		public static void SetupEvaluator() => throw new NotImplementedException(nameof(SetupEvaluator));

		public static ParseResult<T> Parse(string input)
		{
            T value = new();

            foreach (var setter in _parsableSetters)
            {
                var result = setter(value, input);
                if (!result.Successful) return ParseResult<T>.Fail(result.LeftoverInput, result.Error!.Message ?? "Error occured during parsing");
				input = result.LeftoverInput;
            }
            
            var matchResult = _regexEvaluator.Match(input);
            if (matchResult.IsError) return ParseResult<T>.Fail(matchResult)!;
			
            foreach ((var _, NamedGroupMatchValue match) in matchResult.Matches)
            {
                var setterResult = _setters[match.Name](value, match);
				if (!setterResult.Successful) return ParseResult<T>.Fail(matchResult);
            }
            
            return ParseResult<T>.Success(value, matchResult.LeftoverInput);
		}

        protected static void BindSetterToRegexGroup<P>(Expression<Func<T,P>> exp, Func<T, NamedGroupMatchValue, Result> setter)
        {
            var memberName = GetMemberName<P>(exp);
            _setters.Add(memberName, setter);
        }

		protected static void BindParsablePropertySetters<P>(Func<T, string, ParseResult<P>> setter) where P : Parsable<P>, new()
		{
            _parsableSetters.Add(setter);
		}
		
        protected static string GetMemberName<P>(Expression<Func<T,P>> exp)
			=> ExpressionHelpers.GetMemberName(exp);

        protected static Result ReturnResultBasedOnMatchType<M>(T instance, Action<T,M> setter, NamedGroupMatchValue matchValue) where M : NamedGroupMatchValue
        {
            if (matchValue is M value) {
				setter(instance, value);
                return Result.Success();
            } else return Errors.Parsing("Invalid match value for {matchValue.Name}");
        }
	}
}
