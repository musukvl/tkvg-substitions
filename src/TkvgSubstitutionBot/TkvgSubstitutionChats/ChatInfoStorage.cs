namespace TkvgSubstitutionChats;

public interface IChatInfoStorage<T> where T : IChatInfo, new()
{
    public abstract Task SetChatInfo(long chatId, T chatInfo);

    public abstract Task<T> GetChatInfo(long chatId);
    
    public abstract Task DeleteChatInfo(long chatId);
}
