using System.Text.Json;
using System.Text.Json.Serialization;

namespace OSPC.Utils
{
    public class Settings
    {
        public required DataSettings Data { get; set; }
        public required string Token { get; set; }
        public required LoggingSettings Logging { get; set; }
        public required OsuWebApiSettings OsuWebApi { get; set; }
        public required CacheSettings Cache { get; set; }

        public static Settings LoadFromFile(string path)
            => JsonSerializer.Deserialize<Settings>(File.ReadAllText(path))!;
    }

    public class DataSettings
    {
        public required string MySqlConnection { get; set; }
        public required string RedisConnection { get; set; }
    }

    public enum LogLevel
    {
        Critical,
        Error,
        Warning,
        Info,
    }

    public class LoggingSettings
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogLevel LogLevel { get; set; }
        public bool LogToFile { get; set; }
        public required string LogDirectory { get; set; }
        public required string LogFilePath { get; set; }
    }

    public class OsuWebApiSettings
    {
        public int ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public required string BaseUrl { get; set; }
    }
    
    public class CacheSettings
    {
        public int DefaultExpirationTime { get; set; }
    }
}