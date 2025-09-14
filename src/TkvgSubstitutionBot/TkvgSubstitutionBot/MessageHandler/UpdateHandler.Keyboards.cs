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
                .AddButton("3A", "next_day_substitutions:3.a")
                .AddButton("4B", "next_day_substitutions:4.b")
                .AddNewRow()
                .AddButton("Все", "next_day_substitutions:all")
            ;
        return await bot.SendMessage(msg.Chat, "Замены на следующий день. Выберите класс:", replyMarkup: inlineMarkup);
    }
    
    private async Task<Message> CurrentDaySubstitutionKeyboard(Message msg)
    {
        var inlineMarkup = new InlineKeyboardMarkup()
                .AddNewRow()
                .AddButton("3A", "today_substitutions:3.a")
                .AddButton("4B", "today_substitutions:4.b")
                .AddNewRow()
                .AddButton("Все", "today_substitutions:all")
            ;
        return await bot.SendMessage(msg.Chat, "Замены на сегодня. Выберите класс:", replyMarkup: inlineMarkup);
    }
    
    private async Task<Message> SubscriptionKeyboard(Message msg)
    {
        var inlineMarkup = new InlineKeyboardMarkup()
                .AddNewRow()
                .AddButton("3A", "add_subscription:3.a")
                .AddButton("4B", "add_subscription:4.b")
                .AddNewRow()
                .AddButton("Все", "add_subscription:all")
            ;
        return await bot.SendMessage(msg.Chat, "Выберите класс:", replyMarkup: inlineMarkup);
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
