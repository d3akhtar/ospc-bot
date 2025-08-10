using Discord.Interactions;
using OSPC.Bot.Command;
using OSPC.Bot.Component;
using OSPC.Utils;

namespace OSPC.Bot.Module.Interaction
{
    public class SettingsInteractionModule : InteractionModule
    {
        private readonly IBotCommandService _botCmds;

        public SettingsInteractionModule(IBotCommandService botCmds)
        {
            _botCmds = botCmds;
        }

        [SlashCommand("load", "Load beatmap playcounts for a user")]
        public async Task LoadBeatmapPlaycounts(string username = "")
            => await RespondBotCommandResultAsync(await _botCmds.LoadBeatmapPlaycounts(Context.GetOsuContext(), username));

        [SlashCommand("link-profile", "Link your osu profile")]
        public async Task LinkProfile(string username)
            => await RespondBotCommandResultAsync(await _botCmds.LoadBeatmapPlaycounts(Context.GetOsuContext(), username));

        [SlashCommand("help", "Get help about commands and what they do")]
        public async Task Help(string command = "")
        {
            if (string.IsNullOrEmpty(command)) await RespondAsync(embed: Embeded.BuildHelpEmbed(), ephemeral: true);
            else await RespondAsync(embed: Embeded.BuildHelpEmbedForCommand(command), ephemeral: true);
        }
    }
}
