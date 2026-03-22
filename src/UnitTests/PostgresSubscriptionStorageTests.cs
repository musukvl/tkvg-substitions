using Microsoft.EntityFrameworkCore;
using TkvgSubstitutionBot.Data;
using TkvgSubstitutionBot.Subscription;

namespace UnitTests;

public class PostgresSubscriptionStorageTests : IAsyncLifetime
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=tkvgbot;Username=tkvgbot;Password=tkvgbot";

    private AppDbContext _db = null!;
    private PostgresSubscriptionStorage _storage = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        _db = new AppDbContext(options);

        // Clean subscriptions table before each test
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM subscriptions");

        _storage = new PostgresSubscriptionStorage(_db);
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task AddSubscription_CreatesNewSubscription()
    {
        await _storage.AddSubscription(100, "3.a");

        var subs = await _db.Subscriptions.Where(s => s.ChatId == 100).ToListAsync();
        Assert.Single(subs);
        Assert.Equal("3.a", subs[0].ClassName);
    }

    [Fact]
    public async Task AddSubscription_MultipleClasses_CreatesMultipleSubscriptions()
    {
        await _storage.AddSubscription(100, "3.a");
        await _storage.AddSubscription(100, "4.b");

        var subs = await _db.Subscriptions.Where(s => s.ChatId == 100).ToListAsync();
        Assert.Equal(2, subs.Count);
        Assert.Contains(subs, s => s.ClassName == "3.a");
        Assert.Contains(subs, s => s.ClassName == "4.b");
    }

    [Fact]
    public async Task AddSubscription_Duplicate_DoesNotCreateSecond()
    {
        await _storage.AddSubscription(100, "3.a");
        await _storage.AddSubscription(100, "3.a");

        var subs = await _db.Subscriptions.Where(s => s.ChatId == 100).ToListAsync();
        Assert.Single(subs);
    }

    [Fact]
    public async Task AddSubscription_All_RemovesSpecificSubscriptions()
    {
        await _storage.AddSubscription(100, "3.a");
        await _storage.AddSubscription(100, "4.b");
        await _storage.AddSubscription(100, "all");

        var subs = await _db.Subscriptions.Where(s => s.ChatId == 100).ToListAsync();
        Assert.Single(subs);
        Assert.Equal("all", subs[0].ClassName);
    }

    [Fact]
    public async Task AddSubscription_SpecificWhenAllExists_IsNoOp()
    {
        await _storage.AddSubscription(100, "all");
        await _storage.AddSubscription(100, "3.a");

        var subs = await _db.Subscriptions.Where(s => s.ChatId == 100).ToListAsync();
        Assert.Single(subs);
        Assert.Equal("all", subs[0].ClassName);
    }

    [Fact]
    public async Task RemoveAllSubscriptions_DeletesAll()
    {
        await _storage.AddSubscription(100, "3.a");
        await _storage.AddSubscription(100, "4.b");

        await _storage.RemoveAllSubscriptions(100);

        var subs = await _db.Subscriptions.Where(s => s.ChatId == 100).ToListAsync();
        Assert.Empty(subs);
    }

    [Fact]
    public async Task RemoveAllSubscriptions_DoesNotAffectOtherUsers()
    {
        await _storage.AddSubscription(100, "3.a");
        await _storage.AddSubscription(200, "3.a");

        await _storage.RemoveAllSubscriptions(100);

        var subs = await _db.Subscriptions.ToListAsync();
        Assert.Single(subs);
        Assert.Equal(200, subs[0].ChatId);
    }

    [Fact]
    public async Task GetSubscriptionsForClass_ReturnsMatchingAndAll()
    {
        await _storage.AddSubscription(100, "3.a");
        await _storage.AddSubscription(200, "4.b");
        await _storage.AddSubscription(300, "all");

        var subs = await _storage.GetSubscriptionsForClass("3.a");

        Assert.Equal(2, subs.Count);
        Assert.Contains(subs, s => s.ChatId == 100);
        Assert.Contains(subs, s => s.ChatId == 300);
    }

    [Fact]
    public async Task GetSubscriptionsForClass_RingSuffix_MatchesBaseClass()
    {
        await _storage.AddSubscription(100, "3.a");

        var subs = await _storage.GetSubscriptionsForClass("3.a_ring");

        Assert.Single(subs);
        Assert.Equal(100, subs[0].ChatId);
    }

    [Fact]
    public async Task UpdateLastMessage_UpdatesCorrectSubscription()
    {
        await _storage.AddSubscription(100, "3.a");
        var sub = await _db.Subscriptions.FirstAsync(s => s.ChatId == 100);

        await _storage.UpdateLastMessage(sub.Id, "test message");

        // Reload from DB
        var updated = await _db.Subscriptions.AsNoTracking().FirstAsync(s => s.Id == sub.Id);
        Assert.Equal("test message", updated.LastMessage);
    }

    [Fact]
    public async Task AddSubscription_DifferentUsers_SameClass()
    {
        await _storage.AddSubscription(100, "3.a");
        await _storage.AddSubscription(200, "3.a");

        var subs = await _storage.GetSubscriptionsForClass("3.a");
        Assert.Equal(2, subs.Count);
    }

    [Fact]
    public async Task GetSubscriptionsForChat_ReturnsAllForUser()
    {
        await _storage.AddSubscription(100, "3.a");
        await _storage.AddSubscription(100, "4.b");
        await _storage.AddSubscription(200, "3.a");

        var subs = await _storage.GetSubscriptionsForChat(100);
        Assert.Equal(2, subs.Count);
        Assert.Contains(subs, s => s.ClassName == "3.a");
        Assert.Contains(subs, s => s.ClassName == "4.b");
    }

    [Fact]
    public async Task GetSubscriptionsForChat_EmptyForUnknownUser()
    {
        var subs = await _storage.GetSubscriptionsForChat(999);
        Assert.Empty(subs);
    }

    [Fact]
    public async Task RemoveSubscription_RemovesSingleClass()
    {
        await _storage.AddSubscription(100, "3.a");
        await _storage.AddSubscription(100, "4.b");

        await _storage.RemoveSubscription(100, "3.a");

        var subs = await _storage.GetSubscriptionsForChat(100);
        Assert.Single(subs);
        Assert.Equal("4.b", subs[0].ClassName);
    }

    [Fact]
    public async Task RemoveSubscription_DoesNotAffectOtherUsers()
    {
        await _storage.AddSubscription(100, "3.a");
        await _storage.AddSubscription(200, "3.a");

        await _storage.RemoveSubscription(100, "3.a");

        var subs = await _storage.GetSubscriptionsForChat(200);
        Assert.Single(subs);
    }
}
