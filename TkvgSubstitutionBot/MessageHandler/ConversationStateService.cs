using System.Collections.Concurrent;

namespace TkvgSubstitutionBot.MessageHandler;

public class ConversationStateService
{
    private readonly ConcurrentDictionary<long, string> _pendingActions = new();

    public void SetPending(long chatId, string operation)
        => _pendingActions[chatId] = operation;

    public string? GetAndRemovePending(long chatId)
        => _pendingActions.TryRemove(chatId, out var op) ? op : null;

    public void Cancel(long chatId)
        => _pendingActions.TryRemove(chatId, out _);
}
