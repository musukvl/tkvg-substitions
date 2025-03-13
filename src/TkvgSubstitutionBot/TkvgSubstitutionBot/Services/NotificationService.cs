namespace TkvgSubstitutionBot.Services;

public record ChatInfo
{
    public required long ChatId { get; set; }
    public required string ClassName { get; set; }
}

public class NotificationService
{
    private Dictionary<string, ChatInfo> ChatInfos { get; } = new();
    
    public void SetChatInfo(long chatId, string className)
    {
        
        ChatInfos[className] = new ChatInfo
        {
            ChatId = chatId,
            ClassName = className
        };
    }
}