namespace TkvgSubstitutionBot.Subscription;

public class ChatServiceService
{
    private Dictionary<string, ChatInfo> Subscriptions { get; } = new();
    
    public void SetChatInfo(long chatId, string className)
    {
        
        Subscriptions[className] = new ChatInfo
        {
            ChatId = chatId,
            ClassName = className
        };
    }
}