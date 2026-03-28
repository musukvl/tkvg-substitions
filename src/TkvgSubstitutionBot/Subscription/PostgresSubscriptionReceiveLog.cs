using TkvgSubstitutionBot.Data;

namespace TkvgSubstitutionBot.Subscription;

public class PostgresSubscriptionReceiveLog : ISubscriptionReceiveLog
{
    private readonly AppDbContext _db;

    public PostgresSubscriptionReceiveLog(AppDbContext db)
    {
        _db = db;
    }

    public async Task AppendAsync(long chatId, string className, string messageText, CancellationToken cancellationToken = default)
    {
        _db.SubscriptionReceiveLogs.Add(new SubscriptionReceiveLogEntity
        {
            ChatId = chatId,
            ClassName = className,
            MessageText = messageText,
            ReceivedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);
    }
}
