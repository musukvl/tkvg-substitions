
using TkvgSubstitution;

namespace UnitTests
{
    public class TkvgSubstitutionServiceTest
    {
        [Fact]
        public async Task Test1()
        {
            var client = new TkvgSubstitutionService();
            var value = await client.GetSubstitutions("2025-02-17");
            value = value;
        }
    }
}