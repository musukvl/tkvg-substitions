using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using TkvgSubstitutionBot.MessageHandler;

namespace TkvgSubstitutionBot.BackgroundServices;

public class BotBackgroundService(IServiceProvider serviceProvider, ILogger<BotBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bot = serviceProvider.GetRequiredService<ITelegramBotClient>();
        var telegramBot = (TelegramBotClient)bot;

        var me = await bot.GetMe(stoppingToken);
        logger.LogInformation("Starting bot: {BotName}", me.Username);

        await bot.SetMyCommands(
        [
            new() { Command = "next_day_substitutions", Description = "Замены на следующий учебный день" },
            new() { Command = "today_substitutions", Description = "Замены на сегодня" },
            new() { Command = "subscribe", Description = "Подписаться на уведомления о заменах" },
            new() { Command = "unsubscribe", Description = "Отписаться от уведомлений" },
        ], cancellationToken: stoppingToken);

        telegramBot.OnMessage += async (msg, type) =>
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<UpdateHandler>();
                await handler.HandleMessage(msg);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling message from chat {ChatId}", msg.Chat.Id);
            }
        };

        telegramBot.OnUpdate += async update =>
        {
            if (update.CallbackQuery is { } callbackQuery)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<UpdateHandler>();
                    await handler.HandleCallbackQuery(callbackQuery);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error handling callback query {CallbackQueryId}", callbackQuery.Id);
                }
            }
        };

        telegramBot.OnError += (ex, source) =>
        {
            logger.LogError(ex, "Bot error from {Source}", source);
            return Task.CompletedTask;
        };

        // Polling starts automatically when event handlers are attached
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
