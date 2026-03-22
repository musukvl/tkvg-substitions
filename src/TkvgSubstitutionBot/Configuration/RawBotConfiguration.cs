namespace TkvgSubstitutionBot.Configuration;

/// <summary>
/// Raw configuration bound directly from appsettings.yml
/// </summary>
public class RawBotConfiguration
{
    public required string BotToken { get; init; }
    public required string SubstitutionsCheckPeriod { get; init; }
    public required int SilentPeriodStartHour { get; init; }
    public required int SilentPeriodEndHour { get; init; }

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

        if (SilentPeriodStartHour < 0 || SilentPeriodStartHour > 23)
        {
            throw new ArgumentException($"SilentPeriodStartHour must be between 0 and 23, got: {SilentPeriodStartHour}");
        }

        if (SilentPeriodEndHour < 0 || SilentPeriodEndHour > 23)
        {
            throw new ArgumentException($"SilentPeriodEndHour must be between 0 and 23, got: {SilentPeriodEndHour}");
        }

        return new BotConfiguration
        {
            BotToken = BotToken,
            SubstitutionsCheckPeriod = checkPeriod,
            SilentPeriodStartHour = SilentPeriodStartHour,
            SilentPeriodEndHour = SilentPeriodEndHour
        };
    }
}