namespace OSPC.Utils
{
    public record Result(bool Successful, Error? Error)
    {
        public static Result Success() => new(true, Error: null);
        public static Result Fail(Error error) => new(false, error);

        public static implicit operator Result(Error error) => new(false, error);
    }

    public record Result<T> : Result
    {
        public T? Value { get; }

        private Result(T value) : base(true, Error: null) => Value = value;
        private Result(Error error) : base(false, error) { }

        public static Result<T> Success(T value) => new(value);
        public static new Result<T> Fail(Error error) => new(error);

        public static implicit operator Result<T>(T value) => new(value);
        public static implicit operator Result<T>(Exception exception) => new(Errors.Exceptional(exception));
        public static implicit operator Result<T>(Error error)
            => !Errors.UnexpectedErrorForResultType<T>(error) ?
                    new(error) :
                    throw new UnexpectedErrorForResultTypeException($"Unexpected error type {error.Type} for result type {typeof(T).Name}");
    }

    public static class Errors
    {
        private static readonly Dictionary<Type, HashSet<ErrorType>> _expectedErrorsForResultType = new()
        {
            { typeof(int), new() { ErrorType.NotFound }}
        };

        public static Error Unspecified(string message) => new(ErrorType.Unspecified, message);
        public static Error NotFound(string? message = null) => new(ErrorType.NotFound, message);
        public static Error InvalidArguments(string? message = null) => new(ErrorType.InvalidArguments, message);
        public static Error Parsing(string message) => new(ErrorType.Parsing, message);
        public static Exceptional Exceptional(Exception exception) => new(exception);

        public static bool UnexpectedErrorForResultType<T>(Error error)
        {
            return false;
            // Type type = typeof(T);
            // if (!_expectedErrorsForResultType.ContainsKey(type)) 
            //     throw new ArgumentException($"Results of Type {type} aren't expected to have errors currently");
            // else
            //     return !_expectedErrorsForResultType[type].Contains(error.Type);
        }
    }

    public enum ErrorType
    {
        Unspecified,
        NotFound,
        InvalidArguments,
        Parsing,
        Exceptional
    }

    public record Error(ErrorType Type, string? Message = null);
    public record Exceptional(Exception Exception) : Error(ErrorType.Exceptional);

    public class UnexpectedErrorForResultTypeException : Exception
    {
        public UnexpectedErrorForResultTypeException() { }
        public UnexpectedErrorForResultTypeException(string message) : base(message) { }
        public UnexpectedErrorForResultTypeException(string message, Exception inner) : base(message, inner) { }
    }
}
