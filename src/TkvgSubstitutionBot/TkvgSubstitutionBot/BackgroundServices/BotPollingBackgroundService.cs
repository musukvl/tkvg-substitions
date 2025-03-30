using Telegram.Bot;
using Telegram.Bot.Types;
using TkvgSubstitutionBot.BotServices;

namespace TkvgSubstitutionBot.BackgroundServices;

/// <summary>
/// Background service that polls for updates
/// </summary>
public class BotPollingBackgroundService : BackgroundService 
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BotPollingBackgroundService> _logger;

    /// <summary>
    /// Background service that polls for updates
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public BotPollingBackgroundService(IServiceProvider serviceProvider, ILogger<BotPollingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting polling service");
        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        var bot = _serviceProvider.GetRequiredService<ITelegramBotClient>();
        await bot.SetMyCommands(new[]
        {
            new BotCommand { Command = "next_day_substitutions", Description = "Next working day substitutions" },
            new BotCommand { Command = "today_substitutions", Description = "Today substitutions" },
            new BotCommand { Command = "subscribe", Description = "Subscribe notifications" },
            new BotCommand { Command = "unsubscribe", Description = "Unsubscribe notifications" },
            
        });
        
        
        // Make sure we receive updates until Cancellation Requested
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Start receiving updates");
                // Create new IServiceScope on each iteration. This way we can leverage benefits
                // of Scoped TReceiverService and typed HttpClient - we'll grab "fresh" instance each time
                using var scope = _serviceProvider.CreateScope();
                var receiver = scope.ServiceProvider.GetRequiredService<MessageReceiverService>();
                await receiver.ReceiveAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Polling failed with exception: {Exception}", ex);
                // Cooldown if something goes wrong
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
