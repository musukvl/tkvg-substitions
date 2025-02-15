
namespace TkvgSubstitution;

public class TkvgHttpClient
{
    private readonly HttpClient _httpClient;

    public TkvgHttpClient()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string> GetSubstitutionsHtml(string date)
    {
        var request = """
                      {"__args":[null,{"date":"yyyy-mm-dd","mode":"classes"}],"__gsh":"00000000"}
                      """;
        request = request.Replace("yyyy-mm-dd", date);
        var response = await _httpClient.PostAsync("\nhttps://tkvg.edupage.org/substitution/server/viewer.js?__func=getSubstViewerDayDataHtml", new StringContent(request));
        var result = await response.Content.ReadAsStringAsync();
        return result;
    }
}