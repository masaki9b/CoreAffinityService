using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace CoreAffinityService;

public class AffinityModel
{
    private readonly ILogger<AffinityModel> logger;
    private readonly FrozenDictionary<string, ProcessNameAffinityRule> processAffinities;
    private readonly ImmutableArray<FolderPathAffinityRule> folderAffinities;

    public AffinityModel(ILogger<AffinityModel> logger, IOptions<AffinityConfiguration> options)
    {
        this.logger = logger;
        
         var profileDictionary = options.Value.AffinityProfiles.ToFrozenDictionary(
             profile => profile.Name, profile=> Convert.ToUInt64(profile.AffinityMask, 16));
         
        processAffinities = options.Value.ProcessAffinities
            .Where( affinity => profileDictionary.ContainsKey(affinity.Profile) )
            .ToFrozenDictionary( affinity => affinity.ProcessName,
            affinity =>
            {
                var affinityMask = profileDictionary[affinity.Profile];
                return new ProcessNameAffinityRule(affinity, affinityMask);
            });

        folderAffinities = [
            ..options.Value.FolderAffinities
                .Where(affinity => profileDictionary.ContainsKey(affinity.Profile))
                .Select(affinity =>
                {
                    var affinityMask = profileDictionary[affinity.Profile];
                    return new FolderPathAffinityRule(affinity.FolderPath, affinityMask, affinity.DelayDuration);
                })
        ];
    }

    public async ValueTask ApplyAffinityAsync(int processId, string processName, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation($"Find ProcessId '{processId}' ProcessName '{processName}'.");
            var process = Process.GetProcessById(processId);

            if (processAffinities.TryGetValue(process.ProcessName, out var processRule))
            {
                await Task.Delay(processRule.DelayDuration, cancellationToken);
                process.ProcessorAffinity = (IntPtr)processRule.AffinityMask;
                logger.LogInformation($"Affinity applied to process '{process.ProcessName}'.");
                return;
            }

            foreach (var folderAffinity in folderAffinities)
            {
                if (process.MainModule == null)
                {
                    continue;
                }

                if (!process.MainModule.ModuleName.Contains(folderAffinity.FolderPath,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                await Task.Delay(folderAffinity.DelayDuration, cancellationToken);
                process.ProcessorAffinity = (IntPtr)folderAffinity.AffinityMask;
                logger.LogInformation($"Affinity applied to process '{process.ProcessName}'.");
                return;
            }
        }
        catch (ArgumentException)
        {
            // ignore
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to apply affinity.");
        }
    }

    private readonly struct ProcessNameAffinityRule(ProcessAffinityConfig config, ulong affinityMask)
    {
        public readonly ulong AffinityMask = affinityMask;

        public readonly TimeSpan DelayDuration = TimeSpan.FromSeconds(config.DelayDuration);
    }

    private readonly struct FolderPathAffinityRule(string folderPath, ulong affinityMask, int delayDuration)
    {
        public readonly string FolderPath = folderPath;

        public readonly ulong AffinityMask = affinityMask;

        public readonly TimeSpan DelayDuration = TimeSpan.FromSeconds(delayDuration);
    }
}