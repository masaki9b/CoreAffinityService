using System.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.Management.Infrastructure;
using R3;

namespace CoreAffinityService;

public class AffinityManager(
    ILogger<AffinityManager> logger,
    IOptions<AffinityConfiguration> options,
    AffinityModel affinityModel)
    : IDisposable
{
    private readonly CimSession cimSession = CimSession.Create(null);

    public void StartWatching(CancellationToken cancellationToken)
    {
        logger.LogDebug($"Load configuration {options.Value}");

        var cimResults = cimSession.SubscribeAsync(@"root\cimv2", "WQL", "SELECT * FROM Win32_ProcessStartTrace");
        cimResults.ToObservable().SubscribeAwait(async (result, token) =>
        {
            var processIdProperty = result.Instance.CimInstanceProperties["ProcessID"];
            var processNameProperty = result.Instance.CimInstanceProperties["ProcessName"];
            if (processIdProperty is null || processNameProperty is null)
            {
                return;
            }
            
            var processId = Convert.ToInt32(processIdProperty.Value);
            try
            {
                var process = Process.GetProcessById(processId);
                var processName = (string)processNameProperty.Value;
                logger.LogDebug($"Detect process start up '{processId}' ProcessName '{processName}'.");
                if (affinityModel.TryGetAffinityMask(process, out var delay, out var affinityMask))
                {
                    await Task.Delay(delay, token);
                    process.ProcessorAffinity = (IntPtr)affinityMask;
                    logger.LogInformation(
                        $"Affinity 0x{affinityMask:X16} applied to process '{process.ProcessName}'");
                }
                else
                {
                    logger.LogDebug($"Affinity for the '{processName}' is not defined.");
                }
            }
            catch (ArgumentException)
            {
                // ignore
            }
            catch (Exception e)
            {
                logger.LogError(e, "An exception occurred.");
            }
        }, AwaitOperation.Parallel).RegisterTo(cancellationToken);
    }
    
    public void Dispose()
    {
        cimSession.Dispose();
    }
}