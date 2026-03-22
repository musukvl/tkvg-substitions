using Microsoft.Extensions.Options;
using TkvgSubstitution;
using TkvgSubstitution.Models;
using TkvgSubstitutionBot.Configuration;
using Utils = TkvgSubstitution.Utils;

namespace TkvgSubstitutionBot.Subscription;

public class PeriodicalCheckService
{
    private readonly ILogger<PeriodicalCheckService> _logger;
    private readonly TkvgSubstitutionService _substitutionService;
    private readonly NotificationService _notificationService;
    private readonly IOptions<BotConfiguration> _botConfiguration;

    public PeriodicalCheckService(ILogger<PeriodicalCheckService> logger,
        TkvgSubstitutionService substitutionService,
        NotificationService notificationService,
        IOptions<BotConfiguration> botConfiguration)
    {
        _logger = logger;
        _substitutionService = substitutionService;
        _notificationService = notificationService;
        _botConfiguration = botConfiguration;
    }
    
    public async Task DoCheck()
    {
        if (IsInSilentPeriod(DateTime.Now.Hour,
                _botConfiguration.Value.SilentPeriodStartHour,
                _botConfiguration.Value.SilentPeriodEndHour))
        {
            _logger.LogDebug(
                "Skipping check in silent period ({StartHour}:00-{EndHour}:59)",
                _botConfiguration.Value.SilentPeriodStartHour,
                _botConfiguration.Value.SilentPeriodEndHour);
            return;
        }
        var nextWorkingDay = Utils.GetNextWorkingDay(DateTime.Now).ToString("yyyy-MM-dd");
        
        var nextDaySubstitutions = await _substitutionService.GetSubstitutions(nextWorkingDay);
        
        var previousSubstitutions = GetPreviousCheck();
        foreach (var nextDaySubstitution in nextDaySubstitutions)
        {
            var previousSubstitution = previousSubstitutions.FirstOrDefault(x => x.Key == nextDaySubstitution.ClassName);
            if (previousSubstitution.Value == null || previousSubstitution.Value != nextDaySubstitution)
            {
                _logger.LogInformation("Substitution changed for class {ClassName}", nextDaySubstitution.ClassName);
                await _notificationService.Notify(nextDaySubstitution);
            }
        }
        
        var nextDayDict = nextDaySubstitutions.ToDictionary(k => k.ClassName, v => v);
        SaveCheck(nextDayDict);
    }

    
    // 
    private static Dictionary<string, ClassSubstitutions> _previousSubstitutions = new Dictionary<string, ClassSubstitutions>();
    
    private Dictionary<string, ClassSubstitutions> GetPreviousCheck()
    {
        return _previousSubstitutions;
    }

    private void SaveCheck(Dictionary<string, ClassSubstitutions> previousSubstitutions)
    {
        _previousSubstitutions = previousSubstitutions;   
    }

    private static bool IsInSilentPeriod(int currentHour, int silentStartHour, int silentEndHour)
    {
        if (silentStartHour <= silentEndHour)
        {
            return currentHour >= silentStartHour && currentHour <= silentEndHour;
        }

        return currentHour >= silentStartHour || currentHour <= silentEndHour;
    }

}