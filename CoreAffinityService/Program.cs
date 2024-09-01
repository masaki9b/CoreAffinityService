namespace CoreAffinityService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices((hostContext, services) =>
        {
            var configRoot = hostContext.Configuration;
            services.Configure<AffinityConfiguration>(configRoot.GetSection(nameof(AffinityConfiguration)))
                .AddSingleton<AffinityModel>()
                .AddSingleton<AffinityManager>()
                .AddHostedService<AffinityService>();
        });

        var host = builder.Build();
        host.Run();
    }
}