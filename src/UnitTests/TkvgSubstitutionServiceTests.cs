using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using TkvgSubstitution;
using TkvgSubstitution.Configuration;
using TkvgSubstitution.Models;

namespace UnitTests;

public class TkvgSubstitutionServiceTests
{
    private readonly ILogger<TkvgSubstitutionService> _logger;
    private readonly IMemoryCache _cache;
    private readonly IOptions<SubstitutionCacheSettings> _cacheSettings;
    private readonly TkvgSubstitutionService _service;
    private readonly TkvgSubstitutionReader _substitutionReader;

    public TkvgSubstitutionServiceTests()
    {
        _logger = Substitute.For<ILogger<TkvgSubstitutionService>>();
        var tkvgHttpClientLogger = Substitute.For<ILogger<TkvgHttpClient>>();
        _substitutionReader = new TkvgSubstitutionReader(new TkvgHttpClient(new HttpClient() { BaseAddress = new Uri("https://tkvg.edupage.org/") }), tkvgHttpClientLogger);
        _cache = new MemoryCache(new MemoryCacheOptions());
        _cacheSettings = Options.Create(new SubstitutionCacheSettings { CacheDuration = TimeSpan.FromMinutes(30) });
        _service = new TkvgSubstitutionService(_logger, _substitutionReader, _cache, _cacheSettings);
    }

    [Fact]
    public async Task GetSubstitutions_ShouldReturnSubstitutionsFromReader_WhenNotCached()
    {
        // Arrange
        var date = "2025-05-22";
        
        var result = await _service.GetSubstitutions(date);

        // Assert
        Assert.NotNull(result);
    }

 
} 