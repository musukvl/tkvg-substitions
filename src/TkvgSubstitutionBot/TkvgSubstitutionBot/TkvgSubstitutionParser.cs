using HtmlAgilityPack;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using TkvgSubstitutionBot.Models;

namespace TkvgSubstitutionBot;

public class TkvgSubstitutionService
{
    public async Task<List<ClassSubstitutions>> GetSubstitutions(string date)
    {
        var client = new TkvgHttpClient();
        var result = await client.GetSubstitutionsHtml(date);
        var jsonNode = JsonNode.Parse(result);
        var content = jsonNode["r"]?.ToString();

        return ParseSubstitutionsHtml(content);
    }

    private List<ClassSubstitutions> ParseSubstitutionsHtml(string html)
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
                Substitutions = substitutions
            });
        }

        return classSubstitutions;
    }
}