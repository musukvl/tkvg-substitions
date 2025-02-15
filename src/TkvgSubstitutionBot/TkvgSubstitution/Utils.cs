namespace TkvgSubstitution;

public static class Utils 
{
    public static DateTime GetNextWorkingDay(DateTime date)
    {
        return date.DayOfWeek switch
        {
            DayOfWeek.Friday => date.AddDays(3),
            DayOfWeek.Saturday => date.AddDays(2),
            _ => date.AddDays(1)
        };
    }
}