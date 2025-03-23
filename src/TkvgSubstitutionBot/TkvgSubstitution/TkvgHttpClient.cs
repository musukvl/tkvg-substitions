namespace TkvgSubstitution;

public class TkvgHttpClient
{
    private readonly HttpClient _httpClient;

    public TkvgHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetSubstitutionsHtml(string date)
    {
        var request = """
                      {"__args":[null,{"date":"yyyy-mm-dd","mode":"classes"}],"__gsh":"00000000"}
                      """;
        request = request.Replace("yyyy-mm-dd", date);
        var response = await _httpClient.PostAsync("/substitution/server/viewer.js?__func=getSubstViewerDayDataHtml", new StringContent(request));
        var result = await response.Content.ReadAsStringAsync();
        
        return result;
    }
}