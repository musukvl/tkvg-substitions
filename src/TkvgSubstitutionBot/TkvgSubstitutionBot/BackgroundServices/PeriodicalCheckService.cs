using Telegram.Bot;
using TkvgSubstitutionBot.Subscription;

namespace TkvgSubstitutionBot.BackgroundServices;

public class PeriodicalCheckService : BackgroundService
{
    private readonly ILogger<PeriodicalCheckService> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly ChatInfoFileStorage _chatInfoFileStorage;

    public PeriodicalCheckService(ILogger<PeriodicalCheckService> logger, ITelegramBotClient botClient, ChatInfoFileStorage chatInfoFileStorage)
    {
        _logger = logger;
        _botClient = botClient;
        _chatInfoFileStorage = chatInfoFileStorage;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting periodic check at: {time}", DateTimeOffset.Now);
                
                // Place your hourly action here
                await DoPeriodicWork();
                
                // Wait for 1 hour before the next execution
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while executing periodic check");
                // Wait for a shorter time if there was an error before retrying
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task DoPeriodicWork()
    {
        var chats = _chatInfoFileStorage.GetChatIds();
        foreach (var chatId in chats)
        {
            var chatInfo = await _chatInfoFileStorage.GetChatInfo(chatId);
            if (chatInfo == null) continue;
            
            
        }
    }
}