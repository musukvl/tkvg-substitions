namespace TkvgSubstitutionBot.Subscription;

public interface ISubscriptionReceiveLog
{
    Task AppendAsync(long chatId, string className, string messageText, CancellationToken cancellationToken = default);
}
