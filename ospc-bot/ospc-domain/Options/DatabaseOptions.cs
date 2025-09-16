namespace OSPC.Domain.Options
{
    public class DatabaseOptions : IAppOptions
    {
        public static string GetOptionName() => "Database";

        public required string ConnectionString { get; set; }
    }
}