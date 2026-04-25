using TkvgSubstitutionBot.Data;

namespace TkvgSubstitutionBot.Subscription;

public interface ISubscriptionStorage
{
    Task AddSubscription(long chatId, string className);
    Task RemoveAllSubscriptions(long chatId);
    Task RemoveSubscription(long chatId, string className);
    Task<List<SubscriptionEntity>> GetSubscriptionsForClass(string className);
    Task<List<SubscriptionEntity>> GetSubscriptionsForChat(long chatId);
    Task UpdateLastMessage(int subscriptionId, string message);
}
