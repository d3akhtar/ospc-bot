using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
using OSPC.Domain.Options;
using OSPC.Utils;

namespace OSPC.Bot.Logging
{
    public class LoggingService
    {
        private readonly LogSeverity _level;
        private readonly LoggingOptions _options;
        private readonly string _basePath;
        public LoggingService(
            DiscordSocketClient client, 
            CommandService command, 
            InteractionService interactionService,
            LoggingOptions options)
        {
            _basePath = Directory.GetCurrentDirectory();

            client.Log += LogAsync;
            command.Log += LogAsync;
            interactionService.Log += LogAsync;

            _level = options.LogLevel switch {
                LogLevel.Critical => LogSeverity.Critical,
                LogLevel.Error => LogSeverity.Error,
                LogLevel.Warning => LogSeverity.Warning,
                LogLevel.Info => LogSeverity.Info,
                _ => LogSeverity.Debug
            };

            _options = options;
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

            if (_options.LogToFile) 
                File.AppendAllText(
                    Path.Join(_basePath, _options.LogFilePath), 
                    output + "\n");

            return Task.CompletedTask;
        }
    }
}
