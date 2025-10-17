using Discord.Interactions;

using Microsoft.Extensions.Logging;

using OSPC.Bot.Component;
using OSPC.Bot.Extensions;
using OSPC.Bot.Service;
using OSPC.Utils;

namespace OSPC.Bot.Module.Interaction
{
    public class SettingsInteractionModule : InteractionModule
    {
        private readonly IBotCommandService _botCmds;

        public SettingsInteractionModule(ILogger<SettingsInteractionModule> logger, IBotCommandService botCmds)
        {
            _logger = logger;
            _botCmds = botCmds;
        }

        [SlashCommand("load", "Load beatmap playcounts for a user")]
        public async Task LoadBeatmapPlaycounts(string username = Unspecified.User)
            => await RespondBotCommandResultAsync(await _botCmds.LoadBeatmapPlaycounts(Context.GetOsuContext(), username));

        [SlashCommand("link-profile", "Link your osu profile")]
        public async Task LinkProfile(string username)
            => await RespondBotCommandResultAsync(await _botCmds.LoadBeatmapPlaycounts(Context.GetOsuContext(), username));

        [SlashCommand("help", "Get help about commands and what they do")]
        public async Task Help(string command = Unspecified.BotCommand)
        {
            if (command is Unspecified.BotCommand)
                await RespondAsync(embed: EmbededUtils.BuildHelpEmbed(), ephemeral: true);
            else
                await RespondAsync(embed: EmbededUtils.BuildHelpEmbedForCommand(command), ephemeral: true);
        }
    }
}
