using Microsoft.Extensions.Logging.EventLog;

namespace CoreAffinityService;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.AddEventLog(config =>
                    {
                        config.LogName = "Core Affinity Service";
                        config.SourceName = "Core Affinity Service Source";
                    })
                    .AddFilter<EventLogLoggerProvider>(level => LogLevel.Information <= level);
            }).ConfigureServices((hostContext, services) =>
            {
                var configRoot = hostContext.Configuration;
                services.Configure<AffinityConfiguration>(configRoot.GetSection(nameof(AffinityConfiguration)))
                    .AddSingleton<AffinityModel>()
                    .AddSingleton<AffinityManager>()
                    .AddHostedService<AffinityService>();
            }).UseWindowsService();

        var host = builder.Build();
        await host.RunAsync();
        return 0;
    }
}