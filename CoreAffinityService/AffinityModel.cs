using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace CoreAffinityService;

public class AffinityModel
{
    private readonly FrozenDictionary<string, ProcessNameRule> targetProcessRules;
    private readonly ImmutableArray<FolderPathRule> targetFolderRules;

    public AffinityModel(IOptions<AffinityConfiguration> options)
    {
        var profileDictionary = options.Value.AffinityProfiles.ToFrozenDictionary(
            profile => profile.Name, profile => Convert.ToUInt64(profile.AffinityMask, 16));

        targetProcessRules = options.Value.TargetProcessRules
            .Where(rule => profileDictionary.ContainsKey(rule.Profile))
            .ToFrozenDictionary(rule => rule.ProcessName,
                affinity =>
                {
                    var affinityMask = profileDictionary[affinity.Profile];
                    return new ProcessNameRule(affinity, affinityMask);
                });

        targetFolderRules =
        [
            ..options.Value.TargetFolderRules
                .Where(affinity => profileDictionary.ContainsKey(affinity.Profile))
                .Select(affinity =>
                {
                    var affinityMask = profileDictionary[affinity.Profile];
                    return new FolderPathRule(affinity.FolderPath, affinityMask, affinity.DelayDuration);
                })
        ];
    }

    public bool TryGetAffinityMask(Process process, out TimeSpan delay, out ulong affinityMask)
    {
        if (targetProcessRules.TryGetValue(process.ProcessName, out var rule))
        {
            delay = rule.DelayDuration;
            affinityMask = rule.AffinityMask;
            return true;
        }

        foreach (var folderPathRule in targetFolderRules)
        {
            if (process.MainModule == null)
            {
                continue;
            }

            if (!process.MainModule.FileName.Contains(folderPathRule.FolderPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            delay = folderPathRule.DelayDuration;
            affinityMask = folderPathRule.AffinityMask;
            process.ProcessorAffinity = (IntPtr)folderPathRule.AffinityMask;
            return true;
        }

        delay = default;
        affinityMask = default;
        return false;
    }

    private readonly struct ProcessNameRule(TargetProcessRuleConfig config, ulong affinityMask)
    {
        public readonly ulong AffinityMask = affinityMask;

        public readonly TimeSpan DelayDuration = TimeSpan.FromSeconds(config.DelayDuration);
    }

    private readonly struct FolderPathRule(string folderPath, ulong affinityMask, int delayDuration)
    {
        public readonly string FolderPath = folderPath;

        public readonly ulong AffinityMask = affinityMask;

        public readonly TimeSpan DelayDuration = TimeSpan.FromSeconds(delayDuration);
    }
}