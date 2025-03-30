using Microsoft.Extensions.Options;
using Telegram.Bot;
using TkvgSubstitution;
using TkvgSubstitution.Configuration;
using TkvgSubstitutionBot.BackgroundServices;
using TkvgSubstitutionBot.BotServices;
using TkvgSubstitutionBot.Configuration;
using TkvgSubstitutionBot.Subscription;
using PeriodicalCheckService = TkvgSubstitutionBot.Subscription.PeriodicalCheckService;
using UpdateHandler = TkvgSubstitutionBot.MessageHandler.UpdateHandler;
using Serilog;

// use web application just to have /health endpoint for running in container environment
var builder = WebApplication.CreateBuilder(args);

// Add Serilog configuration
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// yaml instead of json just because yaml is more human-readable
builder.Configuration.AddYamlFile("appsettings.yml", optional: false, reloadOnChange: true);
builder.Configuration.AddYamlFile("appsettings.local.yml", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddHealthChecks();

// Configuration
builder.Services.Configure<BotConfiguration>(options =>
{
    var rawConfig = builder.Configuration.GetSection("BotConfiguration").Get<RawBotConfiguration>();
    if (rawConfig == null)
    {
        throw new ArgumentNullException("BotConfiguration section is missing in appsettings.yml");
    }
    var parsedConfig = rawConfig.Parse();
    options.BotToken = parsedConfig.BotToken;
    options.SubstitutionsCheckPeriod = parsedConfig.SubstitutionsCheckPeriod;
    
});

builder.Services.Configure<SubstitutionCacheSettings>(options =>
{    
    var durationStr = builder.Configuration.GetValue<string>("SubstitutionCache:Duration");
    if (!string.IsNullOrEmpty(durationStr))
    {
        options.CacheDuration = ConfigurationParseUtils.ParseTimeSpan(durationStr);
    }
});

// Register named HttpClient to benefits from IHttpClientFactory and consume it with ITelegramBotClient typed client.
// See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#typed-clients
// and https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
builder.Services.AddHttpClient("telegram_bot_client").RemoveAllLoggers()
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        BotConfiguration botConfiguration = (sp.GetRequiredService<IOptions<BotConfiguration>>()).Value;
        TelegramBotClientOptions options = new(botConfiguration.BotToken);
        return new TelegramBotClient(options, httpClient);
    });

builder.Services.AddHttpClient<TkvgHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://tkvg.edupage.org/");
});

// TkvgSubstitution
builder.Services.AddMemoryCache(); 
builder.Services.AddSingleton<TkvgSubstitutionReader>();
builder.Services.AddSingleton<TkvgSubstitutionService>();

// BotServices
// ReceiverService -> UpdateHandler
builder.Services.AddScoped<MessageReceiverService>();
builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddSingleton<ChatInfoFileStorage>();
builder.Services.AddSingleton<SubstitutionFrontendService>();

builder.Services.AddHostedService<BotPollingBackgroundService>();
builder.Services.AddHostedService<TkvgSubstitutionBot.BackgroundServices.PeriodicalCheckService>();

// Notification Sevice
builder.Services.AddSingleton<PeriodicalCheckService>();
builder.Services.AddSingleton<NotificationService>();

var app = builder.Build();


app.Run();
