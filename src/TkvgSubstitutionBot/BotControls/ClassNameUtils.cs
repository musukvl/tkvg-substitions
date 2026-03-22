using System.Text.RegularExpressions;

namespace TkvgSubstitutionBot.BotControls;

public static partial class ClassNameUtils
{
    [GeneratedRegex(@"^\d+[a-zA-Z]$")]
    private static partial Regex ClassNamePattern();

    /// <summary>
    /// Normalizes class name to storage format: "3A", "3a", "3.a", "3.A" → "3.a"
    /// </summary>
    public static string Normalize(string input)
    {
        var trimmed = input.Trim().Replace(".", "");
        if (!ClassNamePattern().IsMatch(trimmed))
            throw new ArgumentException($"Invalid class name: {input}");
        var digits = trimmed[..^1];
        var letter = trimmed[^1..].ToLowerInvariant();
        return $"{digits}.{letter}";
    }

    /// <summary>
    /// Converts storage format to display format: "3.a" → "3A"
    /// </summary>
    public static string ToDisplayFormat(string normalized)
    {
        return normalized.Replace(".", "").ToUpperInvariant();
    }

    /// <summary>
    /// Validates raw user input matches \d+[a-zA-Z] pattern (with optional dot).
    /// </summary>
    public static bool IsValid(string input)
    {
        var trimmed = input.Trim().Replace(".", "");
        return ClassNamePattern().IsMatch(trimmed);
    }
}
