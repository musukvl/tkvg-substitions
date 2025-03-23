namespace TkvgSubstitutionChats;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class ChatInfoFileStorage<T> : IChatInfoStorage<T> where T : IChatInfo, new()
{
    
    private readonly ISerializer _serializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();
    
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public async Task<T> GetChatInfo(long chatId)
    {
        var filePath = GetFilePath(chatId);
        if (!File.Exists(filePath))
        {
            return new T();
        }

        var yaml = await File.ReadAllTextAsync(filePath);
        return _deserializer.Deserialize<T>(yaml);
    }

    public async Task SetChatInfo(long chatId, T chatInfo)
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
    
    private readonly string _storageDirectory = "chat-info";
    private string GetFilePath(long chatId)
    {
        if (!Directory.Exists(_storageDirectory))
        {
            Directory.CreateDirectory(_storageDirectory);
        }
        return Path.Combine(_storageDirectory, $"{chatId}.yaml");
    }
}