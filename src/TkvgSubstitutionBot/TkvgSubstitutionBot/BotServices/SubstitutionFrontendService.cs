using System.Text;
using TkvgSubstitution;
using TkvgSubstitution.Models;
using TkvgSubstitutionBot.Subscription;

namespace TkvgSubstitutionBot.BotServices;

public class SubstitutionFrontendService
{
    private readonly TkvgSubstitutionService _substitutionService;
    private readonly ChatInfoFileStorage _chatInfoFileStorage;

    public SubstitutionFrontendService(TkvgSubstitutionService substitutionService, ChatInfoFileStorage chatInfoFileStorage)
    {
        _substitutionService = substitutionService;
        _chatInfoFileStorage = chatInfoFileStorage;
    }
    
    public async Task<string> GetNextDaySubstitutions(string? className)
    {
        var date = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
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
        result = $"Замены на {date}:\n{result}";
        return result;
    }

    public async Task<string> AddSubscription(long chatId, string className)
    {
        await _chatInfoFileStorage.SetChatInfo(chatId, new ChatInfo
        {
            ChatId = chatId,
            ClassName = className
        });
        return $"Добавлена подписка на замены для {className}. Теперь вы будете получать уведомления о заменах уроков на следующий учебный день.";
    }

    public async Task RemoveSubscription(long chatId)
    {
        await _chatInfoFileStorage.DeleteChatInfo(chatId);
    }
}