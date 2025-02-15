using TkvgSubstitutionBot;

namespace UnitTests;

public class UtilsTest
{
    [Fact]
    public void GetNextWorkingDay_Saturday()
    {
        var date = new DateTime(2025, 2, 15);
        var result = Utils.GetNextWorkingDay(date);
        Assert.Equal(new DateTime(2025, 2, 17), result);
    }
    
    [Fact]
    public void GetNextWorkingDay_Friday()
    {
        var date = new DateTime(2025, 2, 14);
        var result = Utils.GetNextWorkingDay(date);
        Assert.Equal(new DateTime(2025, 2, 17), result);
    }
    
    [Fact]
    public void GetNextWorkingDay_Wednsday()
    {
        var date = new DateTime(2025, 2, 12);
        var result = Utils.GetNextWorkingDay(date);
        Assert.Equal(new DateTime(2025, 2, 13), result);
    }
}