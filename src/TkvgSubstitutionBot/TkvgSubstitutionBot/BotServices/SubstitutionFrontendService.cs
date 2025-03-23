using System.Text;
using TkvgSubstitution;
using TkvgSubstitution.Models;

namespace TkvgSubstitutionBot.BotServices;

public class SubstitutionFrontendService
{
    private readonly TkvgSubstitutionService _substitutionService;

    public SubstitutionFrontendService(TkvgSubstitutionService substitutionService)
    {
        _substitutionService = substitutionService;
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
}