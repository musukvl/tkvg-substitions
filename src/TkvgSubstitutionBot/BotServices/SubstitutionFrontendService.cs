using System.Text;
using TkvgSubstitution;
using TkvgSubstitution.Models;
using TkvgSubstitutionBot.BotControls;
using TkvgSubstitutionBot.Data;
using TkvgSubstitutionBot.Subscription;

namespace TkvgSubstitutionBot.BotServices;

public class SubstitutionFrontendService
{
    private readonly TkvgSubstitutionService _substitutionService;
    private readonly ISubscriptionStorage _subscriptionStorage;

    public SubstitutionFrontendService(TkvgSubstitutionService substitutionService, ISubscriptionStorage subscriptionStorage)
    {
        _substitutionService = substitutionService;
        _subscriptionStorage = subscriptionStorage;
    }
    
    public async Task<string> GetNextDaySubstitutions(string? className)
    {
        var date = TkvgSubstitution.Utils.GetNextWorkingDay(DateTime.Now).ToString("yyyy-MM-dd");
        var substitutions = await _substitutionService.GetSubstitutions(date);
       
        return CreateSubstitutionMessage(substitutions, className, date);
    }
    
    public async Task<string> GetTodaySubstitutions(string? className)
    {
        var date = DateTime.Now.ToString("yyyy-MM-dd");
        var substitutions = await _substitutionService.GetSubstitutions(date);
       
        return CreateSubstitutionMessage(substitutions, className, date);
    }

    public string RenderNotification(ClassSubstitutions classSubstitutions)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Замены {classSubstitutions.ClassName} {classSubstitutions.Date}:");
        foreach (var sub in classSubstitutions.Substitutions)
        {
            sb.AppendLine($"- {sub.Period} ({sub.Info})");
        }
        return sb.ToString();
    }

    private string CreateSubstitutionMessage(List<ClassSubstitutions> substitutions, string? className, string date)
    {
        var sb = new StringBuilder();
        
        var substitutionsFound = false;
        foreach (var substitution in substitutions)
        {
            if (className != null && !string.Equals(substitution.ClassName, className, StringComparison.CurrentCultureIgnoreCase))
                continue;
            substitutionsFound = true;
            if (className == null)
            {
                sb.AppendLine($"# {substitution.ClassName}:");
            }

            foreach (var sub in substitution.Substitutions)
            {
                sb.AppendLine($"- {sub.Period} ({sub.Info})");
            }
        }
        var result = sb.ToString();
        if (!substitutionsFound)
            return $"Нет замен на {date} для {className}.";
        result = $"{date}:\r\n{result}";
        return result;
    }

    public async Task<string> AddSubscription(long chatId, string className)
    {
        await _subscriptionStorage.AddSubscription(chatId, className);
        var display = className == "all" ? "Все" : ClassNameUtils.ToDisplayFormat(className);
        return $"Добавлена подписка на замены для {display}. Теперь вы будете получать уведомления о заменах уроков на следующий учебный день.";
    }

    public async Task RemoveSubscription(long chatId)
    {
        await _subscriptionStorage.RemoveAllSubscriptions(chatId);
    }

    public async Task<(string message, List<SubscriptionEntity> subscriptions)> GetMySubscriptions(long chatId)
    {
        var subs = await _subscriptionStorage.GetSubscriptionsForChat(chatId);
        if (subs.Count == 0)
            return ("У вас нет активных подписок.", subs);
        var sb = new StringBuilder("Ваши подписки:\n");
        foreach (var sub in subs)
        {
            var display = sub.ClassName == "all" ? "Все" : ClassNameUtils.ToDisplayFormat(sub.ClassName);
            sb.AppendLine($"- {display}");
        }
        return (sb.ToString(), subs);
    }

    public async Task<string> RemoveSingleSubscription(long chatId, string className)
    {
        await _subscriptionStorage.RemoveSubscription(chatId, className);
        var display = className == "all" ? "Все" : ClassNameUtils.ToDisplayFormat(className);
        return $"Подписка на {display} отменена.";
    }
}