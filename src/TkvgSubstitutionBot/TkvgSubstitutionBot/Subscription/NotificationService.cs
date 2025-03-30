using Microsoft.Extensions.Options;
using Telegram.Bot;
using TkvgSubstitution.Configuration;
using TkvgSubstitution.Models;
using TkvgSubstitutionBot.BotServices;
using TkvgSubstitutionBot.Configuration;

namespace TkvgSubstitutionBot.Subscription;

public class NotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly ChatInfoFileStorage _chatInfoFileStorage;
    private readonly ITelegramBotClient _botClient;
    private readonly SubstitutionFrontendService _substitutionFrontend;

    public NotificationService(ILogger<NotificationService> logger,
        ChatInfoFileStorage chatInfoFileStorage, ITelegramBotClient botClient, SubstitutionFrontendService substitutionFrontend)
    {
        _logger = logger;
        _chatInfoFileStorage = chatInfoFileStorage;
        _botClient = botClient;
        _substitutionFrontend = substitutionFrontend;
    }
    
    public async Task Notify(ClassSubstitutions classSubstitutions)
    {
        var chatIds = _chatInfoFileStorage.GetChatIds();
        foreach (var chatId in chatIds)
        {
            var chatInfo = await _chatInfoFileStorage.GetChatInfo(chatId);
            if (chatInfo == null || chatInfo.ClassName != classSubstitutions.ClassName) continue;
            
            _logger.LogDebug("Send notification to {ChatId} about class {className}", chatId, classSubstitutions.ClassName);
            var message = _substitutionFrontend.RenderNotification(classSubstitutions);
            await _botClient.SendMessage(chatId, message);
        }
    }
}