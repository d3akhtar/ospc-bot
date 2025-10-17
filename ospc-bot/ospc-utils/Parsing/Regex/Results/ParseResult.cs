using OSPC.Domain.Common;

namespace OSPC.Utils.Parsing.Regex.Results
{
    public record ParseResult : Result
    {
        public string LeftoverInput { get; }

        protected ParseResult(string leftoverInput) : base(true, Error: null) => LeftoverInput = leftoverInput;
        protected ParseResult(string leftoverInput, string error) : base(false, Errors.Parsing(error)) => LeftoverInput = leftoverInput;
    }

    public record ParseResult<T> : ParseResult
    {
        public T? Value { get; }

        private ParseResult(string leftoverInput, T value) : base(leftoverInput) => Value = value;
        private ParseResult(string leftoverInput, string error) : base(leftoverInput, error) { }

        public static ParseResult<T> Success(T value, string leftoverInput) => new(leftoverInput, value);
        public static ParseResult<T> Fail(MatchResult result) => new(result.LeftoverInput, result.ErrorMessage!);
        public static ParseResult<T> Fail(string leftoverInput, string error) => new(leftoverInput, error);

        public static implicit operator ParseResult<T>((string leftoverInput, T value) successResult) => new(successResult.leftoverInput, successResult.value);
        public static implicit operator ParseResult<T>((string leftoverInput, string error) failResult) => new(failResult.leftoverInput, failResult.error);
    }
}