using TkvgSubstitutionBot;

namespace UnitTests;

    public class TkvgTestClient
    {
        [Fact]
        public async Task Test1()
        {
            var client = new TkvgHttpClient();
            var result = await client.GetSubstitutionsHtml("2025-02-17");
            result = result;
        }
    }