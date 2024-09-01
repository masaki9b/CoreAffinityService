namespace CoreAffinityService;

public class AffinityService : BackgroundService
{
    private readonly ILogger<AffinityService> logger;

    private readonly AffinityManager affinityManager;

    public AffinityService(ILogger<AffinityService> logger, AffinityManager affinityManager)
    {
        this.logger = logger;
        this.affinityManager = affinityManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            affinityManager.StartWatching();
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
        }
        
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        stoppingToken.Register(s => (s as TaskCompletionSource<bool>)?.SetResult(true), tcs);
        await tcs.Task;

        logger.LogInformation($"{nameof(AffinityService)} Finished!");
    }
}