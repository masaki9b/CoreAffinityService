using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace CoreAffinityService;

public class AffinityService(ILogger<AffinityService> logger, IOptions<AffinityConfiguration> config, AffinityManager affinityManager)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"{nameof(AffinityService)} is Start");
        var serviceProcess = Process.GetCurrentProcess();
        serviceProcess.ProcessorAffinity = (IntPtr)config.Value.ServiceAffinityMask;
        try
        {
            affinityManager.StartWatching(stoppingToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed process observing.");
        }
        
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        stoppingToken.Register(s => (s as TaskCompletionSource<bool>)?.SetResult(true), tcs);
        await tcs.Task;

        logger.LogInformation($"{nameof(AffinityService)} is Finished");
    }
}