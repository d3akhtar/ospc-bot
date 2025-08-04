using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
using OSPC.Utils;

namespace OSPC.Bot.Logging
{
    public class LoggingService
    {
        private readonly LogSeverity _level;
        private readonly LoggingSettings _settings;
        private readonly string _basePath;
        public LoggingService(
            DiscordSocketClient client, 
            CommandService command, 
            InteractionService interactionService,
            LoggingSettings settings)
        {
            _basePath = Directory.GetCurrentDirectory();

            client.Log += LogAsync;
            command.Log += LogAsync;
            interactionService.Log += LogAsync;

            _level = settings.LogLevel switch {
                LogLevel.Critical => LogSeverity.Critical,
                LogLevel.Error => LogSeverity.Error,
                LogLevel.Warning => LogSeverity.Warning,
                LogLevel.Info => LogSeverity.Info,
                _ => LogSeverity.Debug
            };

            _settings = settings;

            if (!Directory.Exists(Path.Join(_basePath, _settings.LogDirectory))) 
                Directory.CreateDirectory(Path.Join(_basePath, _settings.LogDirectory));
        }

        // TODO: Use serilog, log specific exceptions like MySQLException here
        private Task LogAsync(LogMessage message)
        {
            string output = "";
            if (message.Severity <= _level)
            {
                output += message.Exception switch
                {
                    MySqlException s => $"[Command/{message.Severity}] MySQL error: {s.Message}",
                    CommandException c => $"[Command/{message.Severity}] {c.Command.Aliases.First()}"
                                            + $" failed to execute in {c.Context.Channel}." 
                                            + c.ToString(),
                    _ => $"[General/{message.Severity}] {message}"
                };
            }

            Console.WriteLine(output);

            if (_settings.LogToFile) 
                File.AppendAllText(
                    Path.Join(_basePath, _settings.LogFilePath), 
                    output + "\n");

            return Task.CompletedTask;
        }
    }
}
