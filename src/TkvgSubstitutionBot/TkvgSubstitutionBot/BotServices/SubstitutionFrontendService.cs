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
       
        return CreateSubstitutionMessage(substitutions, className);
    }
    
    public async Task<string> GetTodaySubstitutions(string? className)
    {
        var date = DateTime.Now.ToString("yyyy-MM-dd");
        var substitutions = await _substitutionService.GetSubstitutions(date);
       
        return CreateSubstitutionMessage(substitutions, className);
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

    private string CreateSubstitutionMessage(List<ClassSubstitutions> substitutions, string? className)
    {
        var sb = new StringBuilder();
        foreach (var substitution in substitutions)
        {
            if (className != null && !string.Equals(substitution.ClassName, className, StringComparison.CurrentCultureIgnoreCase))
                continue;
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
        if (string.IsNullOrEmpty(result))
            return "No substitutions.";
        return result;
    }

    public async Task<string> AddSubscription(long chatId, string className)
    {
        await _chatInfoFileStorage.SetChatInfo(chatId, new ChatInfo
        {
            ChatId = chatId,
            ClassName = className
        });
        return "Subscription added.";
    }

    public async Task RemoveSubscription(long chatId)
    {
        await _chatInfoFileStorage.DeleteChatInfo(chatId);
    }

    
}