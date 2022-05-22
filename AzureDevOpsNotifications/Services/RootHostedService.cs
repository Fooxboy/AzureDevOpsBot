namespace AzureDevOpsNotifications.Services;

public class RootHostedService: IHostedService
{
    
    private readonly IConfiguration configuration;
    private readonly IServiceProvider serviceProvider;


    private readonly AzureDevOpsService azureDevOpsService;
    
    public RootHostedService(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        this.configuration = configuration;
        this.serviceProvider = serviceProvider;
        
        var scope = this.serviceProvider.CreateScope();

        this.azureDevOpsService = scope.ServiceProvider.GetRequiredService<AzureDevOpsService>();
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Start bot...");
        var tasks = new[]
        {
             azureDevOpsService.ExecuteTelegramMessagesUpdates(cancellationToken),
             azureDevOpsService.ExecuteReleases(cancellationToken),
             azureDevOpsService.ExecuteFailedBuilds(cancellationToken),
             azureDevOpsService.ExecuteReleasesInProgress(cancellationToken),
             
        };

        await Task.WhenAll(tasks);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;

    }
}