namespace TkvgSubstitutionBot.Subscription;

public record ChatInfo  
{
    public required long ChatId { get; set; }
    public required string ClassName { get; set; }
    
    public string LastMessage { get; set; } = string.Empty;

}