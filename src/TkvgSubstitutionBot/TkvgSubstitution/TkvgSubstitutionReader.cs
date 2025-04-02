using System.Text.Json.Nodes;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using TkvgSubstitution.Models;

namespace TkvgSubstitution;

public class TkvgSubstitutionReader 
{
    private readonly TkvgHttpClient _client;
    private readonly ILogger<TkvgHttpClient> _logger;

    public TkvgSubstitutionReader(TkvgHttpClient client, ILogger<TkvgHttpClient> logger)
    {
        _client = client;
        _logger = logger;
    }
    
    public async Task<List<ClassSubstitutions>> GetSubstitutions(string date)
    {
        var result = await _client.GetSubstitutionsHtml(date);
        _logger.LogDebug("Substitutions received");
        var jsonNode = JsonNode.Parse(result);
        var content = jsonNode["r"]?.ToString();
        return ParseSubstitutionsHtml(content, date);
    }

    private List<ClassSubstitutions> ParseSubstitutionsHtml(string html, string date)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var sections = doc.DocumentNode.SelectNodes("//div[contains(@class, 'section')]");
        var classSubstitutions = new List<ClassSubstitutions>();

        if (sections == null) return classSubstitutions;

        foreach (var section in sections)
        {
            var className = section.SelectSingleNode(".//div[contains(@class, 'header')]")
                ?.SelectSingleNode(".//span")
                ?.InnerText.Trim();

            if (string.IsNullOrEmpty(className)) continue;

            var rows = section.SelectNodes(".//div[contains(@class, 'row')]");
            var substitutions = new List<Substitution>();

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    var period = row.SelectSingleNode(".//div[contains(@class, 'period')]")
                        ?.SelectSingleNode(".//span")
                        ?.InnerText.Trim();

                    var info = row.SelectSingleNode(".//div[contains(@class, 'info')]")
                        ?.SelectSingleNode(".//span")
                        ?.InnerText.Trim();

                    var type = row.GetAttributeValue("class", "").Contains("remove") 
                        ? SubstitutionType.Remove 
                        : SubstitutionType.Change;

                    if (!string.IsNullOrEmpty(period) && !string.IsNullOrEmpty(info))
                    {
                        substitutions.Add(new Substitution
                        {
                            Period = period,
                            Info = info,
                            Type = type
                        });
                    }
                }
            }

            classSubstitutions.Add(new ClassSubstitutions
            {
                ClassName = className,
                Substitutions = substitutions,
                Date = date
            });
        }

        return classSubstitutions;
    }
}
