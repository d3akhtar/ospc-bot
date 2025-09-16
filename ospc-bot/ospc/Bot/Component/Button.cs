using Discord;

namespace OSPC.Bot.Component
{
    public class Button
    {
        public static ButtonBuilder CreateButtonBuilder(string label, string id, bool disabledCondition = false)
            => new ButtonBuilder()
                .WithLabel(label)
                .WithCustomId(id)
                .WithStyle(ButtonStyle.Secondary)
                .WithDisabled(disabledCondition);

        public static MessageComponent GetPageButtonGroup(ulong msgId = 0)
            => new ComponentBuilder()
                .WithRows(
                    new List<ActionRowBuilder>()
                    {
                        new ActionRowBuilder()
                        .WithButton(CreateButtonBuilder("⏪", "first_page", msgId == 0 ? true:BotClient.Instance.CurrentPageForEmbed[msgId] == 1))
                        .WithButton(CreateButtonBuilder("◀️", "prev_page", msgId == 0 ? true:BotClient.Instance.CurrentPageForEmbed[msgId] == 1))
                        .WithButton(CreateButtonBuilder("*️⃣", "choose_page"))
                        .WithButton(CreateButtonBuilder("▶️", "next_page", msgId == 0 ? false:BotClient.Instance.CurrentPageForEmbed[msgId] == Embeded.ActiveEmbeds[msgId].TotalPages))
                        .WithButton(CreateButtonBuilder("⏩", "last_page", msgId == 0 ? false:BotClient.Instance.CurrentPageForEmbed[msgId] == Embeded.ActiveEmbeds[msgId].TotalPages))
                    }
                ).Build();

        public static ButtonType GetButtonTypeFromId(string id)
            => id switch
            {
                "first_page" => ButtonType.FirstPage,
                "prev_page" => ButtonType.PreviousPage,
                "choose_page" => ButtonType.ChoosePage,
                "next_page" => ButtonType.NextPage,
                "last_page" => ButtonType.LastPage,
                _ => ButtonType.Unknown
            };
    }

    public enum ButtonType
    {
        Unknown,
        FirstPage,
        PreviousPage,
        ChoosePage,
        NextPage,
        LastPage
    }
}