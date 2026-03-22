using TkvgSubstitutionBot.Data;

namespace TkvgSubstitutionBot.Subscription;

public interface ISubscriptionStorage
{
    Task AddSubscription(long chatId, string className);
    Task RemoveAllSubscriptions(long chatId);
    Task<List<SubscriptionEntity>> GetSubscriptionsForClass(string className);
    Task UpdateLastMessage(int subscriptionId, string message);
}
