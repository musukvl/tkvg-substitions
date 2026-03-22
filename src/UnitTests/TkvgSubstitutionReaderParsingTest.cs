using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TkvgSubstitution;
using TkvgSubstitution.Models;

namespace UnitTests;

public class TkvgSubstitutionReaderParsingTest
{
    private readonly TkvgSubstitutionReader _reader;
    private readonly TkvgHttpClient _mockClient;

    public TkvgSubstitutionReaderParsingTest()
    {
        _mockClient = Substitute.ForPartsOf<TkvgHttpClient>(new HttpClient());
        var sampleJson = ReadEmbeddedResource("UnitTests.TestData.substitutions_2026-03-23.json");
        _mockClient.GetSubstitutionsHtml(Arg.Any<string>()).Returns(sampleJson);
        _reader = new TkvgSubstitutionReader(_mockClient, NullLogger<TkvgHttpClient>.Instance);
    }

    [Fact]
    public async Task GetSubstitutions_ReturnsAllClasses()
    {
        var result = await _reader.GetSubstitutions("2026-03-23");

        Assert.Equal(22, result.Count);
    }

    [Fact]
    public async Task GetSubstitutions_ClassNamesAreParsedCorrectly()
    {
        var result = await _reader.GetSubstitutions("2026-03-23");

        var classNames = result.Select(r => r.ClassName).ToList();
        Assert.Contains("3.b", classNames);
        Assert.Contains("1.b", classNames);
        Assert.Contains("11._G2.3", classNames);
        Assert.Contains("9.b", classNames);
    }

    [Fact]
    public async Task GetSubstitutions_SingleRemoveRow_ParsedCorrectly()
    {
        var result = await _reader.GetSubstitutions("2026-03-23");

        var class3b = result.Single(r => r.ClassName == "3.b");
        Assert.Single(class3b.Substitutions);
        Assert.Equal("(1)", class3b.Substitutions[0].Period);
        Assert.Equal("2gr: Inglise keel - MARINA ALEKSANDROVITS, Tühistatud", class3b.Substitutions[0].Info);
        Assert.Equal(SubstitutionType.Remove, class3b.Substitutions[0].Type);
    }

    [Fact]
    public async Task GetSubstitutions_MixedRowTypes_ParsedCorrectly()
    {
        var result = await _reader.GetSubstitutions("2026-03-23");

        var class11g23 = result.Single(r => r.ClassName == "11._G2.3");
        Assert.Equal(3, class11g23.Substitutions.Count);

        var removes = class11g23.Substitutions.Where(s => s.Type == SubstitutionType.Remove).ToList();
        var changes = class11g23.Substitutions.Where(s => s.Type == SubstitutionType.Change).ToList();
        Assert.Equal(2, removes.Count);
        Assert.Single(changes);
    }

    [Fact]
    public async Task GetSubstitutions_ClassWithManyRows_AllParsed()
    {
        var result = await _reader.GetSubstitutions("2026-03-23");

        var class9b = result.Single(r => r.ClassName == "9.b");
        Assert.Equal(5, class9b.Substitutions.Count);

        var removes = class9b.Substitutions.Where(s => s.Type == SubstitutionType.Remove).ToList();
        var changes = class9b.Substitutions.Where(s => s.Type == SubstitutionType.Change).ToList();
        Assert.Equal(3, removes.Count);
        Assert.Equal(2, changes.Count);
    }

    [Fact]
    public async Task GetSubstitutions_DateIsSetOnAllResults()
    {
        var result = await _reader.GetSubstitutions("2026-03-23");

        Assert.All(result, r => Assert.Equal("2026-03-23", r.Date));
    }

    [Fact]
    public async Task GetSubstitutions_NoDuplicateRows()
    {
        var result = await _reader.GetSubstitutions("2026-03-23");

        foreach (var classSubstitution in result)
        {
            var duplicates = classSubstitution.Substitutions
                .GroupBy(s => new { s.Period, s.Info, s.Type })
                .Where(g => g.Count() > 1)
                .ToList();

            Assert.Empty(duplicates);
        }
    }

    private static string ReadEmbeddedResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Resource '{resourceName}' not found");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
