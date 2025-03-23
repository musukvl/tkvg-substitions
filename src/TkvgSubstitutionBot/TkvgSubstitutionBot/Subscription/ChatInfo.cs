namespace TkvgSubstitutionBot.Subscription;

public record ChatInfo
{
    public required long ChatId { get; set; }
    public required string ClassName { get; set; }
}