namespace OSPC.Utils
{
	public record Result
	{
		public bool Successful { get; set; }
		public string Error { get; set; }

		protected Result(bool successful, string error)
		{
			Successful = successful;
			Error = error;
		}

		public static Result Success() => new(true, string.Empty);
		public static Result Fail(string error) => new(false, error);

		public static implicit operator Result(string error) => Fail(error);
	}

	public record Result<T> : Result
	{
		public T? Value { get; }

		private Result(T value) : base(true, string.Empty) => Value = value;
		private Result(string error) : base(false, error) {}

		public static implicit operator Result<T>(T value) => new(value);
	    public static implicit operator Result<T>(string error) => new(error);
	}
}
