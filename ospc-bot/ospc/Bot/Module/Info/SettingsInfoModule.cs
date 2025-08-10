using Discord.Commands;
using OSPC.Bot.Command;
using OSPC.Bot.Component;
using OSPC.Utils;

namespace OSPC.Bot.Module.Info
{
    public class SettingsInfoModule : InfoModule
    {
        private readonly IBotCommandService _botCmds;

        public SettingsInfoModule(IBotCommandService botCmds)
        {
            _botCmds = botCmds;
        }

        [Command("load")]
        [Summary("Load beatmap playcounts for a user")]
        public async Task LoadBeatmapPlaycounts(string username = "")
            => await ReplyBotCommandResultAsync(await _botCmds.LoadBeatmapPlaycounts(Context.GetOsuContext(), username));

        [Command("link-profile")]
        public async Task LinkProfile(string username)
            => await ReplyBotCommandResultAsync(await _botCmds.LinkProfile(Context.GetOsuContext(), username));

        [Command("help")]
        public async Task Help(string command = "")
        {
            if (string.IsNullOrEmpty(command)) await ReplyAsync(embed: Embeded.BuildHelpEmbed());
            else await ReplyAsync(embed: Embeded.BuildHelpEmbedForCommand(command));
        }
    }
}
