using System.Runtime.InteropServices.Marshalling;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using TkvgSubstitutionBot.Configuration;
using TkvgSubstitutionBot.Subscription;

namespace TkvgSubstitutionBot.BackgroundServices;

public class PeriodicalCheckBackgroundService : BackgroundService
{
    private readonly ILogger<PeriodicalCheckBackgroundService> _logger;
    private readonly IOptions<BotConfiguration> _botConfiguration;
    private readonly Subscription.PeriodicalCheckService _periodicalCheckService;

    public PeriodicalCheckBackgroundService(ILogger<PeriodicalCheckBackgroundService> logger, IOptions<BotConfiguration> botConfiguration, Subscription.PeriodicalCheckService periodicalCheckService)
    {
        
        _logger = logger;
        _botConfiguration = botConfiguration;
        _periodicalCheckService = periodicalCheckService;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting PeriodicalCheckService every {PeriodicalCheckInterval}", _botConfiguration.Value.SubstitutionsCheckPeriod.ToString());
        _logger.BeginScope("PeriodicalCheckService");
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
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }

    private async Task DoPeriodicWork()
    {
        _logger.LogInformation("Starting periodic check at: {time}", DateTimeOffset.UtcNow);
        await _periodicalCheckService.DoCheck();
        
    }
}