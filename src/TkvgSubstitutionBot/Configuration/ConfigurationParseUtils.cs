namespace TkvgSubstitutionBot.Configuration;

public static class ConfigurationParseUtils
{
    /// <summary>
    /// Parses a string into a TimeSpan. Supports formats like "1h", "30m", "1d", etc.
    /// </summary>
    /// <param name="value">String representation of a time span</param>
    /// <returns>Parsed TimeSpan</returns>
    /// <exception cref="ArgumentException">Thrown when the format is invalid</exception>
    public static TimeSpan ParseTimeSpan(string value)
    {
        if (TryParseTimeSpan(value, out TimeSpan result))
        {
            return result;
        }
        
        throw new ArgumentException($"Invalid TimeSpan format: {value}");
    }
    
    /// <summary>
    /// Tries to parse a string into a TimeSpan. Supports formats like "1h", "30m", "1d", etc.
    /// </summary>
    /// <param name="value">String representation of a time span</param>
    /// <param name="result">Parsed TimeSpan if successful</param>
    /// <returns>True if parsing was successful, false otherwise</returns>
    public static bool TryParseTimeSpan(string value, out TimeSpan result)
    {
        result = TimeSpan.Zero;
        
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }
        
        // Try standard TimeSpan parsing first
        if (TimeSpan.TryParse(value, out result))
        {
            return true;
        }
        
        // Try custom format like "1h", "30m", etc.
        if (value.Length < 2)
        {
            return false;
        }
        
        char unit = value[^1];
        if (!int.TryParse(value[0..^1], out int amount))
        {
            return false;
        }
        
        result = unit switch
        {
            's' => TimeSpan.FromSeconds(amount),
            'm' => TimeSpan.FromMinutes(amount),
            'h' => TimeSpan.FromHours(amount),
            'd' => TimeSpan.FromDays(amount),
            _ => TimeSpan.Zero
        };
        
        return result != TimeSpan.Zero;
    }
}
