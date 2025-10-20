using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OSPC.Bot.Component;
using OSPC.Bot.Context;
using OSPC.Bot.Enums;
using OSPC.Domain.Model;
using OSPC.Infrastructure.Database.Repository;
using OSPC.Infrastructure.Http;
using Serilog;

namespace OSPC.Bot.Messaging.Handlers
{
    // TODO: REMEMBER TO AT LEAST THINK OF A **CLEANER** WAY TO IMPLEMENT THIS LOGIC PLZZ
    public class PagedMessageHandler
    {
        private const int LIMIT = 25;
        private const int BUTTON_DELETE_TIME = 30;

        private readonly ILogger<PagedMessageHandler> _logger;
        private readonly IOsuWebClient _osuWebClient;
        private readonly IBeatmapRepository _beatmapRepo;

        private Dictionary<ulong, int> CurrentPageForEmbed { get; set; } = new();
        private Dictionary<ulong, ButtonType> LastButtonIdClickedForEmbeded { get; set; } = new();
        private Dictionary<ulong, PlaycountEmbedContext> ActiveEmbeds = new();
        private Dictionary<ulong, Timer> ButtonDeleteTimers = new();

        private Dictionary<ulong, PagedMessageContext> activePagedMessages = new();

        public PagedMessageHandler(ILogger<PagedMessageHandler> logger, IOsuWebClient osuWebClient, IBeatmapRepository beatmapRepo)
        {
            _logger = logger;
            _osuWebClient = osuWebClient;
            _beatmapRepo = beatmapRepo;
        }

        public void CreateActivePagedMessage(PlaycountEmbedContext playcountEmbedContext)
        {            
            activePagedMessages.Add(playcountEmbedContext.Message!.Id, new()
            {
                MessageId = playcountEmbedContext.Message!.Id,
                CurrentPageNumber = 1,
                LastPressedButton = ButtonType.Unknown,
                EmbedContext = playcountEmbedContext,
                DeletionTimer = new Timer(async _ =>
                {
                    activePagedMessages[playcountEmbedContext.Message!.Id].DeletionTimer.Dispose();
                    await activePagedMessages[playcountEmbedContext.Message!.Id].DeleteButtons();
                    activePagedMessages.Remove(playcountEmbedContext.Message!.Id);
                })
            });

            activePagedMessages[playcountEmbedContext.Message!.Id].ResetTimer();
        }
        
        public async Task OnModalSubmit(SocketModal modal)
        {
            ulong msgId = modal.Message.Id;
            _logger.LogInformation("Submitting modal for message with id: {MessageId}", msgId);
            var ctx = activePagedMessages[msgId];

            try
            {
                switch (modal.Data.CustomId)
                {
                    case "target_page_number":
                    {
                        await modal.DeferAsync();
                        ctx.CurrentPageNumber = int.Parse(modal.Data.Components.First(x => x.CustomId == "page_number").Value);
                        ctx.LastPressedButton = ButtonType.NextPage;
                        ctx.PauseTimer();
                        await UpdatePageForMessage(ctx);
                        ctx.ResetTimer();
                        break;
                    }
                    default:
                        throw new Exception("Unknown modal");
                }
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Timeout during modal submit for message with id: {MessageId}", msgId);
                ctx.RevertPage();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Something went wrong during modal submit");
            }
        }

        public async Task OnButtonClick(SocketMessageComponent component)
        {
            ulong msgId = component.Message.Id;
            _logger.LogInformation("Clicking button for message with id: {MessageId}, button custom id: {ButtonCustomId}", msgId, component.Data.CustomId);
            var ctx = activePagedMessages[msgId];

            try
            {
                switch (component.Data.CustomId)
                {
                    case "first_page":
                        await component.DeferAsync();
                        ctx.ChangePage(ButtonType.FirstPage);
                        break;
                    case "last_page":
                        await component.DeferAsync();
                        ctx.ChangePage(ButtonType.LastPage);
                        break;
                    case "choose_page":
                        await component.RespondWithModalAsync(modal:
                        new ModalBuilder()
                            .WithTitle("Enter ")
                            .WithCustomId("target_page_number")
                            .AddTextInput("Enter Page Number", "page_number", placeholder: "1").Build());
                        break;
                    case "next_page":
                        await component.DeferAsync();
                        ctx.ChangePage(ButtonType.NextPage);
                        break;
                    case "prev_page":
                        await component.DeferAsync();
                        ctx.ChangePage(ButtonType.PreviousPage);
                        break;
                }
                ctx.PauseTimer();
                await UpdatePageForMessage(ctx);
                ctx.ResetTimer();
            }
            catch (TimeoutException e)
            {
                _logger.LogError(e, "Timeout during modal submit for message with id: {MessageId}", msgId);
                ctx.RevertPage();
            }
        }

        private async Task UpdatePageForMessage(PagedMessageContext pagedMessageContext)
        {
            List<BeatmapPlaycount> mostPlayed = await QueryPlaycountBasedOffEmbedContext(pagedMessageContext.EmbedContext, pagedMessageContext.CurrentPageNumber);
            var stats = await _osuWebClient.GetUserRankStatistics(pagedMessageContext.EmbedContext.User.Id);
            var newEmbed = EmbededUtils.GetEmbedForBeatmapPlaycount(pagedMessageContext.CurrentPageNumber, mostPlayed, pagedMessageContext.EmbedContext.User, pagedMessageContext.EmbedContext, stats);
            await pagedMessageContext.UpdateMessageEmbed(newEmbed);
        }

        private async Task<List<BeatmapPlaycount>> QueryPlaycountBasedOffEmbedContext(PlaycountEmbedContext context, int pageNumber)
        {
            if (!context.Filtered)
            {
                return await _osuWebClient.GetBeatmapPlaycountsForUser
                (context.User.Id, LIMIT, LIMIT * (pageNumber - 1));
            }
            else
            {
                var filterResult = await _beatmapRepo.FilterBeatmapPlaycountsForUser(
                    context.SearchParams!,
                    context.User.Id,
                    LIMIT,
                    pageNumber
                );

                if (!filterResult.Successful)
                    return [];
                else
                    return filterResult.Value!;
            }
        }

        private class PagedMessageContext
        {
            public ulong MessageId { get; set; }
            public int CurrentPageNumber { get; set; }
            public ButtonType LastPressedButton { get; set; }
            public PlaycountEmbedContext EmbedContext { get; set; }
            public Timer DeletionTimer { get; set; }
            
            public void PauseTimer()
            {
                if (DeletionTimer is {})
                {
                    DeletionTimer.Change(dueTime: Timeout.Infinite, period: Timeout.Infinite);
                }
            }

            public void ResetTimer()
            {
                if (DeletionTimer is {})
                {
                    DeletionTimer.Change(dueTime: TimeSpan.FromSeconds(BUTTON_DELETE_TIME), period: TimeSpan.FromSeconds(BUTTON_DELETE_TIME));
                }
            }

            public void ChangePage(ButtonType buttonType)
            {
                switch (buttonType)
                {
                    case ButtonType.FirstPage:
                        CurrentPageNumber = 1;
                        break;
                    case ButtonType.LastPage:
                        CurrentPageNumber = EmbedContext.TotalPages;
                        break;
                    case ButtonType.NextPage:
                        CurrentPageNumber++;
                        break;
                    case ButtonType.PreviousPage:
                        CurrentPageNumber = CurrentPageNumber <= 1 ? 1 : CurrentPageNumber - 1;
                        CurrentPageNumber--;
                        break;
                    default: break;
                }
                
                if (buttonType is not ButtonType.ChoosePage and ButtonType.Unknown)
                {
                    LastPressedButton = buttonType;
                }
            }

            public void RevertPage()
            {
                switch (LastPressedButton)
                {
                    case ButtonType.Unknown:
                        break;
                    case ButtonType.PreviousPage:
                    {
                        CurrentPageNumber++;
                        LastPressedButton = ButtonType.Unknown;
                        break;
                    }
                    case ButtonType.NextPage:
                    {
                        CurrentPageNumber--;
                        LastPressedButton = ButtonType.Unknown;
                        break;
                    }
                }                
            }

            public async Task DeleteButtons()
            {            
                await EmbedContext.Message!.ModifyAsync(
                    msg => msg.Components = new ComponentBuilder().Build(),
                    options: new RequestOptions
                    {
                        Timeout = 5000,
                        RetryMode = RetryMode.RetryRatelimit | RetryMode.RetryTimeouts,
                    }
                );
            }

            public async Task UpdateMessageEmbed(Embed newEmbed)
            {
                await EmbedContext!.Message!.ModifyAsync(
                    msg => { msg.Embed = newEmbed; msg.Components = ButtonUtils.GetPageButtonGroup(MessageId); },
                    options: new RequestOptions
                    {
                        Timeout = 5000,
                        RetryMode = RetryMode.RetryTimeouts,
                    }
                );                
            }
        }
    }
}