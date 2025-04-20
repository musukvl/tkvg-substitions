using Microsoft.Extensions.Options;
using TkvgSubstitutionBot.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TkvgSubstitutionBot.Subscription;


public class ChatInfoFileStorage 
{
    private readonly string _storageDirectory;

    public ChatInfoFileStorage(IOptions<BotConfiguration> botConfiguration)
    {
        var botConfiguration1 = botConfiguration.Value;
        _storageDirectory = Path.Combine(botConfiguration1.ChatInfoDirectory, "subscriptions");
    }
    
    private readonly ISerializer _serializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();
    
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    

    public async Task<ChatInfo?> GetChatInfo(long chatId)
    {
        var filePath = GetFilePath(chatId);
        if (!File.Exists(filePath))
        {
            return null;
        }

        var yaml = await File.ReadAllTextAsync(filePath);
        return _deserializer.Deserialize<ChatInfo>(yaml);
    }

    public async Task SetChatInfo(long chatId, ChatInfo chatInfo)
    {
        var filePath = GetFilePath(chatId);
        var yaml = _serializer.Serialize(chatInfo);
        await File.WriteAllTextAsync(filePath, yaml);
    }

    public async Task DeleteChatInfo(long chatId)
    {
        var filePath = GetFilePath(chatId);
        if (!File.Exists(filePath))
        {
            return;
        }
        File.Delete(filePath);
    }
    
    private string GetFilePath(long chatId)
    {
        EnsureChatInfoDirectory();
        return Path.Combine(_storageDirectory, $"{chatId}.yaml");
    }

    private void EnsureChatInfoDirectory()
    {
        if (!Directory.Exists(_storageDirectory))
        {
            Directory.CreateDirectory(_storageDirectory);
        }
    }

    public List<int> GetChatIds()
    {
        EnsureChatInfoDirectory();
        var files = Directory.GetFiles(_storageDirectory);
        var fileNames=  files
            .Where(x => Path.GetExtension(x) == ".yaml")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
        return fileNames.Select(int.Parse).ToList();
    }
}