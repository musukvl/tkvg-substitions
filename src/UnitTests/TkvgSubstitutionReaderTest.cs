
using Microsoft.Extensions.Logging;
using TkvgSubstitution;

namespace UnitTests
{
    public class TkvgSubstitutionReaderTest
    {
        [Fact]
        public async Task Test1()
        {
            var client = new TkvgSubstitutionReader(new TkvgHttpClient(new HttpClient(){ BaseAddress = new Uri("https://tkvg.edupage.org/") }), new Logger<TkvgHttpClient>(new LoggerFactory()));
            var nextWorkingDay = Utils.GetNextWorkingDay(DateTime.Now).ToString("yyyy-MM-dd");
            var value = await client.GetSubstitutions(nextWorkingDay);
            Assert.NotEmpty(value);
        }
    }
}