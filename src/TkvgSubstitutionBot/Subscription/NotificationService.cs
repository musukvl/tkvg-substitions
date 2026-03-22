using Telegram.Bot;
using TkvgSubstitution.Models;
using TkvgSubstitutionBot.BotServices;

namespace TkvgSubstitutionBot.Subscription;

public class NotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly ISubscriptionStorage _subscriptionStorage;
    private readonly ITelegramBotClient _botClient;
    private readonly SubstitutionFrontendService _substitutionFrontend;

    public NotificationService(ILogger<NotificationService> logger,
        ISubscriptionStorage subscriptionStorage, ITelegramBotClient botClient, SubstitutionFrontendService substitutionFrontend)
    {
        _logger = logger;
        _subscriptionStorage = subscriptionStorage;
        _botClient = botClient;
        _substitutionFrontend = substitutionFrontend;
    }

    public async Task Notify(ClassSubstitutions classSubstitutions)
    {
        var subscriptions = await _subscriptionStorage.GetSubscriptionsForClass(classSubstitutions.ClassName);
        foreach (var subscription in subscriptions)
        {
            _logger.LogDebug("Send notification to {ChatId} about class {className}", subscription.ChatId, classSubstitutions.ClassName);
            var message = _substitutionFrontend.RenderNotification(classSubstitutions);
            if (subscription.LastMessage == message)
                continue;
            await _subscriptionStorage.UpdateLastMessage(subscription.Id, message);
            await _botClient.SendMessage(subscription.ChatId, message);
        }
    }
}
