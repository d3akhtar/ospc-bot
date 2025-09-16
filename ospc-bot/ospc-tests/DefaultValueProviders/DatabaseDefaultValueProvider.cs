using Moq;
using OSPC.Utils;
using MySql.Data.MySqlClient;

namespace OSPC.Tests.DefaultValueProviders
{	
	public class DatabaseDefaultValueProvider : DefaultValueProvider
	{
		protected override object GetDefaultValue(Type type, Mock mock)
	    {
	        if (type == typeof(Result<MySqlCommand>)) return Result<MySqlCommand>.Fail(Errors.Unspecified("Mock"));
	        if (type == typeof(Task<Result<MySqlTransaction>>)) return Task.FromResult(Result<MySqlTransaction>.Fail(Errors.Unspecified("Mock")));
	        return GetDefaultValue(type, mock);
	    }
	}
}
