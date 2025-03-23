using System.Text.RegularExpressions;

namespace TkvgSubstitution;

public static class Utils 
{
    public static DateTime GetNextWorkingDay(DateTime date)
    {
        return date.DayOfWeek switch
        {
            DayOfWeek.Friday => date.AddDays(3),
            DayOfWeek.Saturday => date.AddDays(2),
            _ => date.AddDays(1)
        };
    }
    
    public static TimeSpan ParseTimeSpan(string? str)
    {
        if (string.IsNullOrEmpty(str))
        {
            throw new ArgumentException("Input string cannot be null or empty.", nameof(str));
        }

        var regex = new Regex(@"^(?<value>\d+)(?<unit>[smhd])$", RegexOptions.IgnoreCase);
        var match = regex.Match(str);

        if (!match.Success)
        {
            throw new FormatException("Input string is not in the correct format.");
        }

        var value = int.Parse(match.Groups["value"].Value);
        var unit = match.Groups["unit"].Value.ToLower();

        return unit switch
        {
            "s" => TimeSpan.FromSeconds(value),
            "m" => TimeSpan.FromMinutes(value),
            "h" => TimeSpan.FromHours(value),
            "d" => TimeSpan.FromDays(value),
            _ => throw new FormatException("Invalid time unit.")
        };
    }
}