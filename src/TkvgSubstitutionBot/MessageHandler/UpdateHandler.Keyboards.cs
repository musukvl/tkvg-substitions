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
        var inlineMarkup = ClassPickerKeyboardMarkup.GetClassPickerKeyboard("add_subscription");
        return await bot.SendMessage(msg.Chat, "Выберите класс:", replyMarkup: inlineMarkup);
    }
    
    // Process Inline Keyboard callback data
    public async Task HandleCallbackQuery(CallbackQuery callbackQuery)
    {
        logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        if (string.IsNullOrEmpty(callbackQuery.Data) || !callbackQuery.Data.Contains(':'))
        {
            await bot.AnswerCallbackQuery(callbackQuery.Id, "Invalid callback data");
            return;
        }

        await bot.AnswerCallbackQuery(callbackQuery.Id, "Processing...");

        var parts = callbackQuery.Data.Split(':');
        var operation = parts[0];
        var rawClassName = parts.Length > 1 ? parts[1] : "all";
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
    
    
}
