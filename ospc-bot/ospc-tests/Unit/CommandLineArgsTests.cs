namespace OSPC.Tests.Unit
{
	public class CommandLineArgsTests
	{
		[Theory]
		[InlineData("-c", "false")]
		[InlineData("-c", "true")]
		[InlineData("--caching", "false")]
		[InlineData("--caching", "true")]
		[InlineData("-a", "randomfilepath")]
		[InlineData("--appsettings", "randomfilepath")]
		public void CommandLineArgsParseSuccessTests(params string[] args)
		{
			CommandLineArgs.Parse(args);
		}

		[Theory]
		[InlineData("-c")]
		[InlineData("-c", "true", "--caching")]
		public void CommandLineArgsParseFailWithIndexOutOfRangeExceptionTests(params string[] args)
		{
			CommandLineArgsParseFailWithExceptionTypeTest<IndexOutOfRangeException>(args);
		}

		[Theory]
		[InlineData("-e", "thing")]
		[InlineData("-b", "thing")]
		[InlineData("--invalidflag", "thing")]
		public void CommandLineArgsParseFailWithArgumentExceptionTests(params string[] args)
		{
			CommandLineArgsParseFailWithExceptionTypeTest<ArgumentException>(args);
		}

		[Theory]
		[InlineData("-c", "nottrueorfalse")]
		[InlineData("--caching", "nottrueorfalse")]
		public void CommandLineArgsParseFailWithFormatExceptionTests(params string[] args)
		{
			CommandLineArgsParseFailWithExceptionTypeTest<FormatException>(args);
		}

		private void CommandLineArgsParseFailWithExceptionTypeTest<T>(params string[] args) where T:Exception
		{
			Assert.Throws<T>(() => CommandLineArgs.Parse(args));
		}
	}
}
