using TkvgSubstitutionBot.BotControls;

namespace UnitTests;

public class ClassNameUtilsTests
{
    [Theory]
    [InlineData("3A", "3.a")]
    [InlineData("3a", "3.a")]
    [InlineData("3.a", "3.a")]
    [InlineData("3.A", "3.a")]
    [InlineData("4B", "4.b")]
    [InlineData("11C", "11.c")]
    [InlineData("11.c", "11.c")]
    [InlineData(" 3A ", "3.a")]
    public void Normalize_ValidInput_ReturnsStorageFormat(string input, string expected)
    {
        Assert.Equal(expected, ClassNameUtils.Normalize(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("3")]
    [InlineData("AB")]
    [InlineData("3AB")]
    [InlineData("hello")]
    public void Normalize_InvalidInput_ThrowsArgumentException(string input)
    {
        Assert.Throws<ArgumentException>(() => ClassNameUtils.Normalize(input));
    }

    [Theory]
    [InlineData("3.a", "3A")]
    [InlineData("4.b", "4B")]
    [InlineData("11.c", "11C")]
    public void ToDisplayFormat_ReturnsUpperCaseWithoutDot(string normalized, string expected)
    {
        Assert.Equal(expected, ClassNameUtils.ToDisplayFormat(normalized));
    }

    [Theory]
    [InlineData("3A", true)]
    [InlineData("3a", true)]
    [InlineData("3.a", true)]
    [InlineData("11C", true)]
    [InlineData(" 4B ", true)]
    [InlineData("", false)]
    [InlineData("A", false)]
    [InlineData("3", false)]
    [InlineData("3AB", false)]
    [InlineData("hello", false)]
    public void IsValid_ReturnsExpected(string input, bool expected)
    {
        Assert.Equal(expected, ClassNameUtils.IsValid(input));
    }
}
