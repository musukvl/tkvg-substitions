using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace TkvgSubstitutionBot.Services;

public class UpdateHandler(ITelegramBotClient bot, ILogger<UpdateHandler> logger, SubstitutionFrontendService substitutionService) : IUpdateHandler
{
    private static readonly InputPollOption[] PollOptions = ["Hello", "World!"];

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        logger.LogInformation("HandleError: {Exception}", exception);
        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await (update switch
        {
            { Message: { } message }                        => OnMessage(message),
            { EditedMessage: { } message }                  => OnMessage(message),
            { CallbackQuery: { } callbackQuery }            => OnCallbackQuery(callbackQuery),
            { InlineQuery: { } inlineQuery }                => OnInlineQuery(inlineQuery),
            { ChosenInlineResult: { } chosenInlineResult }  => OnChosenInlineResult(chosenInlineResult),

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
            _ => Usage(msg)
        });
        
        logger.LogInformation("The message was sent with id: {SentMessageId} {ChatId}", sentMessage.Id, msg.Chat.Id);
    }

    async Task<Message> Usage(Message msg)
    {
        const string usage = """
                <b><u>Bot menu</u></b>:
                /next_day_substitutions
                /today_substitutions
            """;
        return await bot.SendMessage(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
    }
    
    async Task<Message> NextDaySubstitutionKeyboard(Message msg)
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
    
    async Task<Message> CurrentDaySubstitutionKeyboard(Message msg)
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
    
    
    // Process Inline Keyboard callback data
    private async Task OnCallbackQuery(CallbackQuery callbackQuery)
    {
        logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
        var operation = callbackQuery.Data?.Split(":")[0];
        var className = callbackQuery.Data?.Split(":")[1];
        className = className == "all" ? null : className;

        string messageResult = operation switch
        {
            "today_substitutions" => await substitutionService.GetTodaySubstitutions(className),
            "next_day_substitutions" => await substitutionService.GetNextDaySubstitutions(className),
            _ => ""
        };
        await bot.AnswerCallbackQuery(callbackQuery.Id, "Processing...");
        await bot.SendMessage(callbackQuery.Message!.Chat, messageResult, parseMode: ParseMode.None);
    }

    #region Inline Mode

    private async Task OnInlineQuery(InlineQuery inlineQuery)
    {
        logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

        InlineQueryResult[] results = [ // displayed result
            new InlineQueryResultArticle("1", "Telegram.Bot", new InputTextMessageContent("hello")),
            new InlineQueryResultArticle("2", "is the best", new InputTextMessageContent("world"))
        ];
        await bot.AnswerInlineQuery(inlineQuery.Id, results, cacheTime: 0, isPersonal: true);
    }

    private async Task OnChosenInlineResult(ChosenInlineResult chosenInlineResult)
    {
        logger.LogInformation("Received inline result: {ChosenInlineResultId}", chosenInlineResult.ResultId);
        await bot.SendMessage(chosenInlineResult.From.Id, $"You chose result with Id: {chosenInlineResult.ResultId}");
    }

    #endregion
    

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
}
