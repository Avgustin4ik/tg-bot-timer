using Telegram.Bot;
using Console.Advanced.Services;
using tg_bot_timer;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddUserSecrets<Program>(optional:true);
    })
    .ConfigureServices((context, services) =>
    {
        const string botTokenPath = "Telegram-Bot";
        var token = context.Configuration[botTokenPath];
        var serverUri = context.Configuration["ApiUrl"];
        services.AddHttpClient<TimerApiClient>(client =>
        {
            client.BaseAddress = new Uri(serverUri); // ← твой сервер
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException(
                "BotToken не найден! Настройте через: dotnet user-secrets set \"BotToken\" \"xxx\"");
        }
        // Register named HttpClient to benefits from IHttpClientFactory and consume it with ITelegramBotClient typed client.
        // See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#typed-clients
        // and https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        services.AddHttpClient("telegram_bot_client").RemoveAllLoggers()
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                TelegramBotClientOptions options = new(token);
                return new TelegramBotClient(options, httpClient);
            });
        services.AddScoped<TimerApiClient>();
        services.AddScoped<UpdateHandler>();
        services.AddScoped<ReceiverService>();
        services.AddHostedService<PollingService>();
    })
    .Build();

await host.RunAsync();