namespace TkvgSubstitutionBot.Services;

public class ChatInfo
{
    public long ChatId { get; set; }
    public string ClassName { get; set; }
}

public class NotificationService
{
    public Dictionary<string, ChatInfo> ChatInfos { get; } = new();
    
    public void SetChatInfo(long chatId, string className)
    {
        
        ChatInfos[className] = new ChatInfo
        {
            ChatId = chatId,
            ClassName = className
        };
    }
}