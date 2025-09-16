using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Sdk;

namespace OSPC.Tests.Attributes
{
	public class JsonFileDataAttribute : DataAttribute
	{
		private readonly string _filePath;
		private string? _propertyName;

		private JsonSerializer _serializer;

		private string _projectRootPath = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;

		public JsonFileDataAttribute(string filePath) : this(filePath, null) {}

		public JsonFileDataAttribute(string filePath, string? propertyName)
		{
			_filePath = filePath;
			_propertyName = propertyName;
			_serializer = JsonSerializer.Create(new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
			    MissingMemberHandling = MissingMemberHandling.Ignore,
			    TypeNameHandling = TypeNameHandling.All,
			    Formatting = Formatting.Indented,
			});
		}

		public override IEnumerable<object[]> GetData(MethodInfo testMethod)
		{
			if (_propertyName is null) _propertyName = testMethod.Name;
			
			if (testMethod is null) throw new ArgumentException(nameof(testMethod));

			ParameterInfo[] paramInfo = testMethod.GetParameters();

			string path = Path.Join(Path.Join(_projectRootPath, GetTestDataFilePathFromNamespace(testMethod.DeclaringType?.Namespace!)), _filePath);

			if (!File.Exists(path)) throw new FileNotFoundException(path);

			var contents = File.ReadAllText(path);

			if (contents.Length == 0) throw new ArgumentException($"Empty contents: {path}");

			var jsonObject = JObject.Parse(contents);
			var jsonProperty = jsonObject[_propertyName] ?? throw new ArgumentException($"Property \'{_propertyName}\' not found: {path}");

			var jsonArray = (JArray)jsonProperty;

			foreach (JArray token in jsonArray)
			{
				object[] args = new object[token.Count];
				for (int i = 0; i < token.Count; i++) args[i] = token[i].ToObject(paramInfo[i].ParameterType, _serializer)!;
				yield return args;
			}
		}

		private string GetTestDataFilePathFromNamespace(string namespaceName)
			=> namespaceName switch {
				"OSPC.Tests.Unit" => "Unit/TestData",
				"OSPC.Tests.Integration" => "Integration/TestData",
				"OSPC.Tests.EndToEnd" => "EndToEnd/TestData",
				_ => throw new ArgumentException(nameof(namespaceName))
			};
	}
}
