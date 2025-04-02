namespace TkvgSubstitution.Models;

public record ClassSubstitutions
{
    public required string ClassName { get; init; }
    public required List<Substitution> Substitutions { get; init; }
    public required string Date { get; init; }
} 