using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TkvgSubstitutionBot.BotServices;

namespace TkvgSubstitutionBot.MessageHandler;

public partial class UpdateHandler(ITelegramBotClient bot, ILogger<UpdateHandler> logger, SubstitutionFrontendService frontend) : IUpdateHandler
{
    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        logger.LogInformation("HandleError: {Exception}", exception);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await (update switch
        {
            { Message: { } message }                        => OnMessage(message),
            { EditedMessage: { } message }                  => OnMessage(message),
            { CallbackQuery: { } callbackQuery }            => OnCallbackQuery(callbackQuery),

            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            _                                               => UnknownUpdateHandlerAsync(update)
        });
    }

    private async Task OnMessage(Message msg)
    {
        logger.LogInformation("Receive message type: {MessageType}", msg.Type);
        if (msg.Text is not { } messageText)
            return;

        Message sentMessage = await (messageText.Split(' ')[0] switch
        {
            "/next_day_substitutions" => NextDaySubstitutionKeyboard(msg),
            "/today_substitutions" => CurrentDaySubstitutionKeyboard(msg),
            "/subscribe" => SubscriptionKeyboard(msg),
            "/unsubscribe" => Unsubscribe(msg),
            _ => Usage(msg)
        });
        
        logger.LogInformation("The message was sent with id: {SentMessageId} {ChatId}", sentMessage.Id, msg.Chat.Id);
    }

    private async Task<Message> Unsubscribe(Message msg)
    {
        await frontend.RemoveSubscription(msg.Chat.Id);
        return await bot.SendMessage(msg.Chat, "Unsubscribed", parseMode: ParseMode.None);
    }
    
    private async Task<Message> Usage(Message msg)
    {
        const string usage = """
                <b><u>Меню бота</u></b>:
                /next_day_substitutions
                /today_substitutions
            """;
        return await bot.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
    }
    
    private Task UnknownUpdateHandlerAsync(Update update)
    {
        logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
}
