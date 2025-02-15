using McMaster.Extensions.CommandLineUtils;
using TkvgSubstitution;

public class Program
{
    [Option(Description = "Class", ShortName = "c", LongName = "class")]
    public string? Class { get; } 
    
    [Option(Description = "Date", ShortName = "d", LongName = "date")]
    public string? Date { get; }

    [Option(Description = "Verbose", ShortName = "v", LongName = "verbose")]
    public bool Verbose { get; } = false;
    
    public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

    private async Task<int> OnExecute()
    {
        var tkvgService = new TkvgSubstitutionService();

        var nextDateString = Date;
        if (string.IsNullOrEmpty(nextDateString))
        {
            var date = Utils.GetNextWorkingDay(DateTime.Now);
            nextDateString = date.Date.ToString("yyyy-MM-dd");
        }

        if (Verbose)
            Console.WriteLine("Date: " + nextDateString);
        if (Class != null)
            if (Verbose)
                Console.WriteLine("Class: " + Class);
        
        var substitutions = await tkvgService.GetSubstitutions(nextDateString);
        foreach (var sub in substitutions)
        {
            if (Class != null && !string.Equals(sub.ClassName, Class, StringComparison.CurrentCultureIgnoreCase))
                continue;
            foreach (var s in sub.Substitutions)
            {
                Console.WriteLine("    " + s.Period + " " + s.Info);
            }
        }

        return 0;
    }
}

