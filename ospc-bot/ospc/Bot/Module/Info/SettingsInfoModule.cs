using Discord.Commands;

using Microsoft.Extensions.Logging;

using OSPC.Bot.Service;
using OSPC.Bot.Component;
using OSPC.Bot.Extensions;
using OSPC.Domain.Constants;

namespace OSPC.Bot.Module.Info
{
    public class SettingsInfoModule : InfoModule
    {
        private readonly IBotCommandService _botCmds;

        public SettingsInfoModule(ILogger<SettingsInfoModule> logger, IBotCommandService botCmds)
        {
            _logger = logger;
            _botCmds = botCmds;
        }

        [Command("load")]
        [Summary("Load beatmap playcounts for a user")]
        public async Task LoadBeatmapPlaycounts(string username = Unspecified.User)
            => await ReplyBotCommandResultAsync(await _botCmds.LoadBeatmapPlaycounts(Context.GetOsuContext(), username));

        [Command("link-profile")]
        public async Task LinkProfile(string username)
            => await ReplyBotCommandResultAsync(await _botCmds.LinkProfile(Context.GetOsuContext(), username));

        [Command("help")]
        public async Task Help(string command = Unspecified.BotCommand)
        {
            if (command is Unspecified.BotCommand)
                await ReplyAsync(embed: EmbededUtils.BuildHelpEmbed());
            else
                await ReplyAsync(embed: EmbededUtils.BuildHelpEmbedForCommand(command));
        }
    }
}
