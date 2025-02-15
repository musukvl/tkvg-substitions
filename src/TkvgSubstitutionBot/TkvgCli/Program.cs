// See https://aka.ms/new-console-template for more information

using TkvgSubstitution;

var tkvgService = new TkvgSubstitutionService();
var nextDate = Utils.GetNextWorkingDay(DateTime.Now);
var nextDateString = nextDate.Date.ToString("yyyy-MM-dd");
Console.WriteLine("Date: " + nextDateString);
var substitutions = await tkvgService.GetSubstitutions(nextDateString);
foreach (var sub in substitutions)
{
    Console.WriteLine(sub.ClassName);
    foreach (var s in sub.Substitutions)
    {
        Console.WriteLine("    " + s.Period + " " + s.Info);
    }
}

