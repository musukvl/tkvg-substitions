using TkvgSubstitution;

namespace UnitTests;

    public class TkvgTestClient
    {
        [Fact]
        public async Task Test1()
        {
            var client = new TkvgHttpClient(new HttpClient() { BaseAddress = new Uri("https://tkvg.edupage.org/") });
            var result = await client.GetSubstitutionsHtml("2025-02-17");
            Assert.NotEmpty(result);
        }
    }