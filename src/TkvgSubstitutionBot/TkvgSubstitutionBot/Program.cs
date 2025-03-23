using Microsoft.Extensions.Options;
using Telegram.Bot;
using TkvgSubstitution;
using TkvgSubstitution.Configuration;
using TkvgSubstitutionBot;
using TkvgSubstitutionBot.BackgroundServices;
using TkvgSubstitutionBot.BotServices;
using UpdateHandler = TkvgSubstitutionBot.MessageHandler.UpdateHandler;

// use web application just to have /health endpoint for running in container environment
var builder = WebApplication.CreateBuilder(args);

// yaml instead of json just because yaml is more human-readable
builder.Configuration.AddYamlFile("appsettings.yml", optional: false, reloadOnChange: true);
builder.Configuration.AddYamlFile("appsettings.local.yml", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddHealthChecks();

builder.Services.Configure<BotConfiguration>(builder.Configuration.GetSection("BotConfiguration"));


// Register named HttpClient to benefits from IHttpClientFactory and consume it with ITelegramBotClient typed client.
// See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#typed-clients
// and https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
builder.Services.AddHttpClient("telegram_bot_client").RemoveAllLoggers()
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        BotConfiguration? botConfiguration = sp.GetService<IOptions<BotConfiguration>>()?.Value;
        ArgumentNullException.ThrowIfNull(botConfiguration);
        TelegramBotClientOptions options = new(botConfiguration.BotToken);
        return new TelegramBotClient(options, httpClient);
    });

builder.Services.AddHttpClient<TkvgHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://tkvg.edupage.org/");
});

builder.Services.Configure<SubstitutionCacheSettings>(options =>
{    
    var durationStr = builder.Configuration.GetValue<string>("SubstitutionCache:Duration");
    if (!string.IsNullOrEmpty(durationStr))
    {
        options.CacheDuration = Utils.ParseTimeSpan(durationStr);
    }
});

 
builder.Services.AddMemoryCache(); 

builder.Services.AddSingleton<TkvgSubstitutionReader>();
builder.Services.AddSingleton<TkvgSubstitutionService>();
builder.Services.AddSingleton<SubstitutionFrontendService>();

// ReceiverService -> UpdateHandler
builder.Services.AddScoped<ReceiverService>();
builder.Services.AddScoped<UpdateHandler>();


builder.Services.AddHostedService<PollingBackgroundService>();
builder.Services.AddHostedService<PeriodicalCheckService>();
 




var app = builder.Build();
app.Run();