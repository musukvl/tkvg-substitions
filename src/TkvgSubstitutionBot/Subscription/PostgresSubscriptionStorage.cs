using Microsoft.EntityFrameworkCore;
using TkvgSubstitutionBot.Data;

namespace TkvgSubstitutionBot.Subscription;

public class PostgresSubscriptionStorage : ISubscriptionStorage
{
    private readonly AppDbContext _db;

    public PostgresSubscriptionStorage(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddSubscription(long chatId, string className)
    {
        if (className == "all")
        {
            // "all" supersedes individual class subscriptions
            await _db.Subscriptions
                .Where(s => s.ChatId == chatId && s.ClassName != "all")
                .ExecuteDeleteAsync();
        }
        else
        {
            // If user already has "all", don't add a specific one
            var hasAll = await _db.Subscriptions
                .AnyAsync(s => s.ChatId == chatId && s.ClassName == "all");
            if (hasAll) return;
        }

        var exists = await _db.Subscriptions
            .AnyAsync(s => s.ChatId == chatId && s.ClassName == className);
        if (!exists)
        {
            _db.Subscriptions.Add(new SubscriptionEntity
            {
                ChatId = chatId,
                ClassName = className
            });
            await _db.SaveChangesAsync();
        }
    }

    public async Task RemoveAllSubscriptions(long chatId)
    {
        await _db.Subscriptions
            .Where(s => s.ChatId == chatId)
            .ExecuteDeleteAsync();
    }

    public async Task<List<SubscriptionEntity>> GetSubscriptionsForClass(string className)
    {
        return await _db.Subscriptions
            .Where(s => s.ClassName == className
                        || s.ClassName + "_ring" == className
                        || s.ClassName == "all")
            .ToListAsync();
    }

    public async Task UpdateLastMessage(int subscriptionId, string message)
    {
        await _db.Subscriptions
            .Where(s => s.Id == subscriptionId)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.LastMessage, message));
    }
}
