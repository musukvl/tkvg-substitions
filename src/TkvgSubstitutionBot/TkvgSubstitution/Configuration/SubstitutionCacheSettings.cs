namespace TkvgSubstitution.Configuration;

public record SubstitutionCacheSettings
{
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(30);
}