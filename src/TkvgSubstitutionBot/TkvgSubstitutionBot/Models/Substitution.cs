namespace TkvgSubstitutionBot.Models;

public class Substitution
{
    public string Period { get; set; }
    public string Info { get; set; }
    public SubstitutionType Type { get; set; }
}

public enum SubstitutionType
{
    Remove,
    Change
} 