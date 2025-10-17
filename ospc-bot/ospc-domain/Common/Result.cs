namespace OSPC.Domain.Common
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
}
