namespace TkvgSubstitutionBot.Configuration;

/// <summary>
/// Validated bot configuration used throughout the application
/// </summary>
public class BotConfiguration
{
    public required string BotToken { get; set; }
    public required TimeSpan SubstitutionsCheckPeriod { get; set; }
    public required string ChatInfoDirectory { get; set; }
}