namespace TkvgSubstitutionBot.Data;

public class SubscriptionEntity
{
    public int Id { get; set; }
    public long ChatId { get; set; }
    public required string ClassName { get; set; }
    public string LastMessage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
