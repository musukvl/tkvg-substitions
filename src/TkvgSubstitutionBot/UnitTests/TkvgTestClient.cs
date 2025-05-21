using TkvgSubstitution;

namespace UnitTests;

public class TkvgTestClient
{
    [Fact]
    public async Task TestGetSubstitution()
    {
        var client = new TkvgHttpClient(new HttpClient() { BaseAddress = new Uri("https://tkvg.edupage.org/") });
        var result = await client.GetSubstitutionsHtml("2025-05-22");
        Assert.NotEmpty(result);
    }
}