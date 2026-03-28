namespace TkvgSubstitutionBot.Data;

public class SubscriptionReceiveLogEntity
{
    public long Id { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
    public required string ClassName { get; set; }
    public long ChatId { get; set; }
    public required string MessageText { get; set; }
}
