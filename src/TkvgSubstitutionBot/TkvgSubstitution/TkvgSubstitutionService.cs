using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TkvgSubstitution.Configuration;
using TkvgSubstitution.Models;

namespace TkvgSubstitution;

public class TkvgSubstitutionService
{
    private readonly ILogger<TkvgSubstitutionService> _logger;
    private readonly TkvgSubstitutionReader _substitutionReader;
    private readonly IMemoryCache _cache;
    private readonly IOptions<SubstitutionCacheSettings> _cacheSettings;

    public TkvgSubstitutionService(ILogger<TkvgSubstitutionService> logger, TkvgSubstitutionReader substitutionReader, IMemoryCache memoryCache, IOptions<SubstitutionCacheSettings> cacheSettings)
    {
        _logger = logger;
        _substitutionReader = substitutionReader;
        _cache = memoryCache;
        _cacheSettings = cacheSettings;
    }

    public async Task<List<ClassSubstitutions>> GetSubstitutions(string date)
    {
        var cacheKey = $"substitutions_{date}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            _logger.LogDebug("Fetching substitutions for date {Date}", date);
            entry.AbsoluteExpirationRelativeToNow = _cacheSettings.Value.CacheDuration;
            return await _substitutionReader.GetSubstitutions(date);
        }) ?? new List<ClassSubstitutions>();
    }
} 