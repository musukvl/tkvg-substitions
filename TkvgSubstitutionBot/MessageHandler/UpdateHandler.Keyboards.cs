using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TkvgSubstitutionBot.BotControls;

namespace TkvgSubstitutionBot.MessageHandler;

public partial class UpdateHandler
{
    private async Task<Message> NextDaySubstitutionKeyboard(Message msg)
    {
        var inlineMarkup = ClassPickerKeyboardMarkup.GetClassPickerKeyboard("next_day_substitutions");
        return await bot.SendMessage(msg.Chat, "Замены на следующий день. Выберите класс:", replyMarkup: inlineMarkup);
    }

    private async Task<Message> CurrentDaySubstitutionKeyboard(Message msg)
    {
        var inlineMarkup = ClassPickerKeyboardMarkup.GetClassPickerKeyboard("today_substitutions");
        return await bot.SendMessage(msg.Chat, "Замены на сегодня. Выберите класс:", replyMarkup: inlineMarkup);
    }

    private async Task<Message> SubscriptionKeyboard(Message msg)
    {
        var inlineMarkup = ClassPickerKeyboardMarkup.GetSubscribeClassPickerKeyboard("add_subscription");
        return await bot.SendMessage(msg.Chat, "Выберите класс:", replyMarkup: inlineMarkup);
    }

    private async Task<Message> MySubscriptionsMessage(Message msg)
    {
        var (message, subscriptions) = await frontend.GetMySubscriptions(msg.Chat.Id);

        if (subscriptions.Count == 0)
            return await bot.SendMessage(msg.Chat, message, parseMode: ParseMode.None);

        var keyboard = new InlineKeyboardMarkup();
        foreach (var sub in subscriptions)
        {
            var display = sub.ClassName == "all" ? "Все" : ClassNameUtils.ToDisplayFormat(sub.ClassName);
            keyboard.AddNewRow().AddButton($"Отписаться {display}", $"remove_subscription:{sub.ClassName}");
        }

        return await bot.SendMessage(msg.Chat, message, replyMarkup: keyboard, parseMode: ParseMode.None);
    }

    public async Task HandleCallbackQuery(CallbackQuery callbackQuery)
    {
        logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        if (string.IsNullOrEmpty(callbackQuery.Data))
        {
            await bot.AnswerCallbackQuery(callbackQuery.Id, "Invalid callback data");
            return;
        }

        // Handle cancel_input (no colon)
        if (callbackQuery.Data == "cancel_input")
        {
            conversationState.Cancel(callbackQuery.From.Id);
            await bot.AnswerCallbackQuery(callbackQuery.Id, "Отменено");
            await bot.SendMessage(callbackQuery.Message!.Chat, "Отменено.", parseMode: ParseMode.None);
            return;
        }

        if (!callbackQuery.Data.Contains(':'))
        {
            await bot.AnswerCallbackQuery(callbackQuery.Id, "Invalid callback data");
            return;
        }

        await bot.AnswerCallbackQuery(callbackQuery.Id, "Processing...");

        var parts = callbackQuery.Data.Split(':', 2);
        var operation = parts[0];
        var payload = parts.Length > 1 ? parts[1] : "";

        switch (operation)
        {
            case "enter_class":
                // payload = the original command (e.g., "next_day_substitutions")
                conversationState.SetPending(callbackQuery.From.Id, payload);
                await bot.SendMessage(callbackQuery.Message!.Chat,
                    "Введите название класса (например, 3A, 4B, 11C):",
                    replyMarkup: new ForceReplyMarkup());
                break;

            case "retry_input":
                // payload = the original command
                conversationState.SetPending(callbackQuery.From.Id, payload);
                await bot.SendMessage(callbackQuery.Message!.Chat,
                    "Введите название класса (например, 3A, 4B, 11C):",
                    replyMarkup: new ForceReplyMarkup());
                break;

            case "remove_subscription":
                // payload = className (e.g., "3.a")
                var removeResult = await frontend.RemoveSingleSubscription(callbackQuery.From.Id, payload);
                await bot.SendMessage(callbackQuery.Message!.Chat, removeResult, parseMode: ParseMode.None);
                break;

            case "today_substitutions":
            case "next_day_substitutions":
            case "add_subscription":
                var rawClassName = payload.Length > 0 ? payload : "all";
                await ProcessCallbackOperation(callbackQuery, operation, rawClassName);
                break;

            default:
                await bot.SendMessage(callbackQuery.Message!.Chat, "Неизвестная команда.", parseMode: ParseMode.None);
                break;
        }
    }

    private async Task ProcessCallbackOperation(CallbackQuery callbackQuery, string operation, string rawClassName)
    {
        // For query commands, "all" means no filter (null). For subscriptions, keep "all" as literal.
        var queryClassName = rawClassName == "all" ? null : rawClassName;

        string messageResult = operation switch
        {
            "today_substitutions" => await frontend.GetTodaySubstitutions(queryClassName),
            "next_day_substitutions" => await frontend.GetNextDaySubstitutions(queryClassName),
            "add_subscription" => await frontend.AddSubscription(callbackQuery.From.Id, rawClassName),
            _ => ""
        };

        await bot.SendMessage(callbackQuery.Message!.Chat, messageResult, parseMode: ParseMode.None);
    }

    internal async Task<Message> ProcessOperationWithClass(Chat chat, string operation, string normalizedClassName)
    {
        // For query commands, pass the class name directly.
        // For subscriptions, pass the normalized name.
        string messageResult = operation switch
        {
            "today_substitutions" => await frontend.GetTodaySubstitutions(normalizedClassName),
            "next_day_substitutions" => await frontend.GetNextDaySubstitutions(normalizedClassName),
            "add_subscription" => await frontend.AddSubscription(chat.Id, normalizedClassName),
            _ => ""
        };

        return await bot.SendMessage(chat, messageResult, parseMode: ParseMode.None);
    }
}
