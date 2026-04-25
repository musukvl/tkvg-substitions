using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TkvgSubstitutionBot.BotControls;
using TkvgSubstitutionBot.BotServices;

namespace TkvgSubstitutionBot.MessageHandler;

public partial class UpdateHandler(
    ITelegramBotClient bot,
    ILogger<UpdateHandler> logger,
    SubstitutionFrontendService frontend,
    ConversationStateService conversationState)
{
    public async Task HandleMessage(Message msg)
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
            "/my_subscriptions" => MySubscriptionsMessage(msg),
            _ => HandleFreeTextOrUsage(msg)
        });

        logger.LogInformation("The message was sent with id: {SentMessageId} {ChatId}", sentMessage.Id, msg.Chat.Id);
    }

    private async Task<Message> Unsubscribe(Message msg)
    {
        await frontend.RemoveSubscription(msg.Chat.Id);
        return await bot.SendMessage(msg.Chat, "Подписка отменена.", parseMode: ParseMode.None);
    }

    private async Task<Message> HandleFreeTextOrUsage(Message msg)
    {
        var chatId = msg.Chat.Id;
        var operation = conversationState.GetAndRemovePending(chatId);

        if (operation == null)
            return await Usage(msg);

        var input = msg.Text!.Trim();
        if (!ClassNameUtils.IsValid(input))
        {
            // Re-set pending so retry works
            conversationState.SetPending(chatId, operation);

            var errorKeyboard = new InlineKeyboardMarkup()
                .AddNewRow()
                .AddButton("Повторить", $"retry_input:{operation}")
                .AddButton("Отмена", "cancel_input");

            return await bot.SendMessage(msg.Chat,
                $"Неверный формат класса: \"{input}\". Введите в формате: цифра + буква (например, 3A, 4B, 11C).",
                replyMarkup: errorKeyboard);
        }

        var normalized = ClassNameUtils.Normalize(input);
        return await ProcessOperationWithClass(msg.Chat, operation, normalized);
    }

    private async Task<Message> Usage(Message msg)
    {
        const string usage = """
                <b><u>Меню бота</u></b>:
                /next_day_substitutions
                /today_substitutions
                /my_subscriptions
            """;
        return await bot.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
    }
}
