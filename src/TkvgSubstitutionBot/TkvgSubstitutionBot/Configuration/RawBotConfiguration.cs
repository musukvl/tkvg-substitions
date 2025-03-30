namespace TkvgSubstitutionBot.Configuration;

/// <summary>
/// Raw configuration bound directly from appsettings.yml
/// </summary>
public class RawBotConfiguration
{
    public required string BotToken { get; init; }
    public required string SubstitutionsCheckPeriod { get; init; }

    /// <summary>
    /// Validates and parses the raw configuration into a strongly-typed BotConfiguration
    /// </summary>
    /// <returns>Validated BotConfiguration</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public BotConfiguration Parse()
    {
        if (!ConfigurationParseUtils.TryParseTimeSpan(SubstitutionsCheckPeriod, out TimeSpan checkPeriod))
        {
            throw new ArgumentException($"Invalid SubstitutionsCheckPeriod format: {SubstitutionsCheckPeriod}");
        }

        return new BotConfiguration
        {
            BotToken = BotToken,
            SubstitutionsCheckPeriod = checkPeriod,
            ChatInfoDirectory = Path.Combine(AppContext.BaseDirectory, "chat-info")
        };
    }
}