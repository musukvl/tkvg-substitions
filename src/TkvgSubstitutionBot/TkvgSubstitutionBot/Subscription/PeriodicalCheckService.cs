using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using TkvgSubstitution;
using TkvgSubstitution.Models;
using TkvgSubstitutionBot.Configuration;
using Utils = TkvgSubstitution.Utils;

namespace TkvgSubstitutionBot.Subscription;

public class PeriodicalCheckService
{
    private readonly ILogger<PeriodicalCheckService> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly ChatInfoFileStorage _chatInfoFileStorage;
    private readonly IOptions<BotConfiguration> _botConfiguration;
    private readonly TkvgSubstitutionService _substitutionService;
    private readonly NotificationService _notificationService;

    public PeriodicalCheckService(ILogger<PeriodicalCheckService> logger, 
        ITelegramBotClient botClient, 
        ChatInfoFileStorage chatInfoFileStorage, 
        IOptions<BotConfiguration> botConfiguration, 
        TkvgSubstitutionService substitutionService,
        NotificationService notificationService)
    {
        
        _logger = logger;
        _botClient = botClient;
        _chatInfoFileStorage = chatInfoFileStorage;
        _botConfiguration = botConfiguration;
        _substitutionService = substitutionService;
        _notificationService = notificationService;
    }
    
    public async Task DoCheck()
    {
        // avoid sending notifications at night
        if (DateTime.Now.Hour < 16 || DateTime.Now.Hour > 23)
        {
            _logger.LogDebug("Skipping check at night");
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

}