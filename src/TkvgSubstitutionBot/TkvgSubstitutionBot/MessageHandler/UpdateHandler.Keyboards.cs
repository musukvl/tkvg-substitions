using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TkvgSubstitutionBot.MessageHandler;

public partial class UpdateHandler
{
    private async Task<Message> NextDaySubstitutionKeyboard(Message msg)
    {
        var inlineMarkup = new InlineKeyboardMarkup()
                .AddNewRow()
                .AddButton("2A", "next_day_substitutions:2a")
                .AddButton("3B", "next_day_substitutions:3b")
                .AddButton("6D", "next_day_substitutions:6d")
                .AddNewRow()
                .AddButton("All", "next_day_substitutions:all")
            ;
        return await bot.SendMessage(msg.Chat, "Next Day Substitutions. Pick Class:", replyMarkup: inlineMarkup);
    }
    
    private async Task<Message> CurrentDaySubstitutionKeyboard(Message msg)
    {
        var inlineMarkup = new InlineKeyboardMarkup()
                .AddNewRow()
                .AddButton("2A", "today_substitutions:2a")
                .AddButton("3B", "today_substitutions:3b")
                .AddButton("6D", "today_substitutions:6d")
                .AddNewRow()
                .AddButton("All", "today_substitutions:all")
            ;
        return await bot.SendMessage(msg.Chat, "Today Substitutions. Pick Class:", replyMarkup: inlineMarkup);
    }
    
    private async Task<Message> SubscriptionKeyboard(Message msg)
    {
        var inlineMarkup = new InlineKeyboardMarkup()
                .AddNewRow()
                .AddButton("2A", "add_subscription:2a")
                .AddButton("3B", "add_subscription:3b")
                .AddButton("6D", "add_subscription:6d")
                .AddNewRow()
                .AddButton("All", "add_subscription:all")
            ;
        return await bot.SendMessage(msg.Chat, "Pick Class:", replyMarkup: inlineMarkup);
    }
    
    // Process Inline Keyboard callback data
    private async Task OnCallbackQuery(CallbackQuery callbackQuery)
    {
        logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
        var operation = callbackQuery.Data?.Split(":")[0];
        var className = callbackQuery.Data?.Split(":")[1];
        className = className == "all" ? null : className;

        string messageResult = operation switch
        {
            "today_substitutions" => await frontend.GetTodaySubstitutions(className),
            "next_day_substitutions" => await frontend.GetNextDaySubstitutions(className),
            "add_subscription" => await frontend.AddSubscription(callbackQuery.From.Id, className),
            _ => ""
        };
        await bot.AnswerCallbackQuery(callbackQuery.Id, "Processing...");
        await bot.SendMessage(callbackQuery.Message!.Chat, messageResult, parseMode: ParseMode.None);
    }
}
