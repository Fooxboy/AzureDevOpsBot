using AzureDevOpsNotifications.Services;
using NLog.Extensions.Logging;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(c =>
    {
        c.SetBasePath(Directory.GetCurrentDirectory());
        c.AddJsonFile("hostsettings.json", optional: true);
        c.AddCommandLine(args);
    })
    .ConfigureAppConfiguration((h, c) =>
    {
        var env = h.HostingEnvironment;

        c.AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true);
        c.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {

        services.AddTransient<TelegramService>();
        services.AddTransient<SmartCatService>();
        services.AddTransient<TranslatorService>();
        services.AddTransient<AzureDevOpsService>();

        services.AddHttpClient<SmartCatService>();
        services.AddHttpClient<TranslatorService>();
        services.AddHostedService<RootHostedService>();
        
        
        services.AddLogging(c =>
        {
            // configure Logging with NLog
            c.ClearProviders();
            c.SetMinimumLevel(LogLevel.Trace);
            c.AddNLog("NLog.Development.config");
        });
    })
    .Build();

await host.RunAsync();
