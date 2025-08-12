using Discord;
using Discord.WebSocket;
using OSPC.Bot.Component;
using Serilog;

namespace OSPC.Bot.MessageHandlers
{
	// TODO: REMEMBER TO AT LEAST THINK OF A **CLEANER** WAY TO IMPLEMENT THIS LOGIC PLZZ
	public class PagedMessageHandler
	{
        public async Task OnModalSubmit(SocketModal modal)
        {
            ulong msgId = modal.Message.Id;
			Log.Information("Submitting modal for message with id: {MessageId}", msgId);
			
            try {
                switch (modal.Data.CustomId) {
                    case "target_page_number": {
                        await modal.DeferAsync();
                        BotClient.Instance.CurrentPageForEmbed[msgId] = int.Parse(modal.Data.Components.First(x => x.CustomId == "page_number").Value);
                        BotClient.Instance.LastButtonIdClickedForEmbeded[msgId] = ButtonType.NextPage;
                        Embeded.PauseTimer(modal.Message);
                        await BotClient.Instance.InvokePageForEmbedUpdatedEvent(msgId);
                        Embeded.ResetTimer(modal.Message);
                        break;
                    }
                    default: throw new Exception("Unknown modal");
                }
            } catch (TimeoutException e) {
				Log.Error(e, "Timeout during modal submit for message with id: {MessageId}", msgId);
                switch (BotClient.Instance.LastButtonIdClickedForEmbeded[msgId]) {
                    case ButtonType.Unknown: break;
                    case ButtonType.PreviousPage: {
                        BotClient.Instance.CurrentPageForEmbed[msgId]++;
                        BotClient.Instance.LastButtonIdClickedForEmbeded[msgId] = ButtonType.Unknown;
                        break;
                    }
                    case ButtonType.NextPage: {
                        BotClient.Instance.CurrentPageForEmbed[msgId]--;
                        BotClient.Instance.LastButtonIdClickedForEmbeded[msgId] = ButtonType.Unknown;
                        break;
                    }
                }
            } catch (Exception e) {
				Log.Error(e, "Something went wrong during modal submit");
			}
        }
		
        public async Task OnButtonClick(SocketMessageComponent component)
        {
            ulong msgId = component.Message.Id;
			Log.Information("Clicking button for message with id: {MessageId}, button custom id: {ButtonCustomId}", msgId, component.Data.CustomId);
			
            try {
                switch (component.Data.CustomId) {
                    case "first_page":
                        await component.DeferAsync();
                        BotClient.Instance.CurrentPageForEmbed[msgId] = 1;
                        BotClient.Instance.LastButtonIdClickedForEmbeded[msgId] = ButtonType.FirstPage;
                        break;
                    case "last_page":
                        await component.DeferAsync();
                        BotClient.Instance.CurrentPageForEmbed[msgId] = Embeded.ActiveEmbeds[msgId].TotalPages;
                        BotClient.Instance.LastButtonIdClickedForEmbeded[msgId] = ButtonType.LastPage;
                        break;
                    case "choose_page":
                        await component.RespondWithModalAsync(modal:  
                        new ModalBuilder()
                            .WithTitle("Enter ")
                            .WithCustomId("target_page_number")
                            .AddTextInput("Enter Page Number", "page_number", placeholder:"1").Build());
                        break;
                    case "next_page":
                        await component.DeferAsync();
                        BotClient.Instance.CurrentPageForEmbed[msgId]++;
                        BotClient.Instance.LastButtonIdClickedForEmbeded[msgId] = ButtonType.NextPage;
                        break;
                    case "prev_page":
                        await component.DeferAsync();
                        BotClient.Instance.CurrentPageForEmbed[msgId] = BotClient.Instance.CurrentPageForEmbed[msgId] <= 1 ? 1:BotClient.Instance.CurrentPageForEmbed[msgId]-1;
                        BotClient.Instance.LastButtonIdClickedForEmbeded[msgId] = ButtonType.PreviousPage;
                        break;
                }
                Embeded.PauseTimer(component.Message);
                await BotClient.Instance.InvokePageForEmbedUpdatedEvent(msgId);
                Embeded.ResetTimer(component.Message);
            } catch (TimeoutException e) {
				Log.Error(e, "Timeout during modal submit for message with id: {MessageId}", msgId);
                switch (BotClient.Instance.LastButtonIdClickedForEmbeded[msgId]) {
                    case ButtonType.Unknown: break;
                    case ButtonType.PreviousPage: {
                        BotClient.Instance.CurrentPageForEmbed[msgId]++;
                        BotClient.Instance.LastButtonIdClickedForEmbeded[msgId] = ButtonType.Unknown;
                        break;
                    }
                    case ButtonType.NextPage: {
                        BotClient.Instance.CurrentPageForEmbed[msgId]--;
                        BotClient.Instance.LastButtonIdClickedForEmbeded[msgId] = ButtonType.Unknown;
                        break;
                    }
                }
            }
        }
	}

}
