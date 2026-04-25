using System.ComponentModel;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Spectre.Console.Cli;
using TkvgSubstitution;
using TkvgSubstitution.Models;

namespace Tkvg.Cli.Commands;

public class SubstitutionsCommandSettings : CommandSettings
{
    [Description("Date in dd.MM format (e.g. 23.03)")]
    [CommandArgument(0, "<date>")]
    public required string Date { get; init; }

    [Description("School class to filter by (e.g. 3b)")]
    [CommandOption("--class|-c")]
    public string? Class { get; init; }
}

public class SubstitutionsCommand : AsyncCommand<SubstitutionsCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, SubstitutionsCommandSettings settings, CancellationToken cancellation)
    {
        if (!DateOnly.TryParseExact(settings.Date, "dd.MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            Console.Error.WriteLine($"Invalid date format: '{settings.Date}'. Expected dd.MM (e.g. 23.03)");
            return 1;
        }

        var date = new DateOnly(DateTime.Now.Year, parsedDate.Month, parsedDate.Day);
        var dateString = date.ToString("yyyy-MM-dd");

        var httpClient = new HttpClient { BaseAddress = new Uri("https://tkvg.edupage.org/") };
        var tkvgClient = new TkvgHttpClient(httpClient);
        var reader = new TkvgSubstitutionReader(tkvgClient, NullLogger<TkvgHttpClient>.Instance);

        var substitutions = await reader.GetSubstitutions(dateString);

        var output = FormatSubstitutions(substitutions, settings.Class, dateString);
        Console.Write(output);

        return 0;
    }

    private static string FormatSubstitutions(List<ClassSubstitutions> substitutions, string? className, string date)
    {
        var sb = new StringBuilder();
        var found = false;

        foreach (var substitution in substitutions)
        {
            if (className != null && !string.Equals(substitution.ClassName, className, StringComparison.CurrentCultureIgnoreCase))
                continue;

            found = true;

            if (className == null)
            {
                sb.AppendLine($"# {substitution.ClassName}:");
            }

            foreach (var sub in substitution.Substitutions)
            {
                sb.AppendLine($"- {sub.Period} ({sub.Info})");
            }
        }

        if (!found)
        {
            return className != null
                ? $"Нет замен на {date} для {className}.\n"
                : $"Нет замен на {date}.\n";
        }

        return $"{date}:\n{sb}";
    }
}
