using Microsoft.Extensions.Options;
using TkvgSubstitutionBot.Configuration;
using TkvgSubstitutionBot.Subscription;

namespace TkvgSubstitutionBot.BackgroundServices;

public class PeriodicalCheckBackgroundService : BackgroundService
{
    private readonly ILogger<PeriodicalCheckBackgroundService> _logger;
    private readonly IOptions<BotConfiguration> _botConfiguration;
    private readonly IServiceScopeFactory _scopeFactory;

    public PeriodicalCheckBackgroundService(ILogger<PeriodicalCheckBackgroundService> logger,
        IOptions<BotConfiguration> botConfiguration,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _botConfiguration = botConfiguration;
        _scopeFactory = scopeFactory;
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

                await DoPeriodicWork();

                await Task.Delay(_botConfiguration.Value.SubstitutionsCheckPeriod, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while executing periodic check");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }

    private async Task DoPeriodicWork()
    {
        _logger.LogInformation("Starting periodic check at: {time}", DateTimeOffset.UtcNow);
        using var scope = _scopeFactory.CreateScope();
        var checkService = scope.ServiceProvider.GetRequiredService<PeriodicalCheckService>();
        await checkService.DoCheck();
    }
}
