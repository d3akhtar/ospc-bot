using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

using MySql.Data.MySqlClient;

using OSPC.Domain.Options;
using OSPC.Utils;

using Serilog;
using Serilog.Events;

namespace OSPC.Bot.Logging
{
    public class LoggingService
    {
        private readonly string _basePath;

        public LoggingService(DiscordSocketClient client, CommandService command, InteractionService interactionService)
        {
            _basePath = Directory.GetCurrentDirectory();

            client.Log += LogAsync;
            command.Log += LogAsync;
            interactionService.Log += LogAsync;
        }

        private async Task LogAsync(LogMessage message)
        {
            var severity = message.Severity switch
            {
                LogSeverity.Critical => LogEventLevel.Fatal,
                LogSeverity.Error => LogEventLevel.Error,
                LogSeverity.Warning => LogEventLevel.Warning,
                LogSeverity.Info => LogEventLevel.Information,
                LogSeverity.Verbose => LogEventLevel.Verbose,
                LogSeverity.Debug => LogEventLevel.Debug,
                _ => LogEventLevel.Information
            };

            Log.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);

            await Task.CompletedTask;
        }
    }
}