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

    public void StartWatching()
    {
        logger.LogInformation($"Load configuration {options.Value}" );
        
        var cimResults = cimSession.SubscribeAsync(@"root\cimv2", "WQL", "SELECT * FROM Win32_ProcessStartTrace");
        cimResults.ToObservable().SubscribeAwait( async (result, token) =>
        {
            var processIdProperty = result.Instance.CimInstanceProperties["ProcessID"];
            var processNameProperty = result.Instance.CimInstanceProperties["ProcessName"];
            if (processIdProperty is not null && processNameProperty is not null)
            {
                var processId =  Convert.ToInt32(processIdProperty.Value);
                var processName = (string)processNameProperty.Value;
                await affinityModel.ApplyAffinityAsync(processId, processName, token);
            }
        });
    }
        

    public void Dispose()
    {
        cimSession.Dispose();
    }
}