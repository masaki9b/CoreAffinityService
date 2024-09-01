using System.Text.Json;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace CoreAffinityService;

public class AffinityConfiguration
{
    public ulong ServiceAffinityMask { get; init; }
    public AffinityProfileConfig[] AffinityProfiles { get; set; } = [];
    public TargetProcessRuleConfig[] TargetProcessRules { get; set; } = [];
    public TargetFolderRuleConfig[] TargetFolderRules { get; set; } = [];

    public override string ToString()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true 
        };
        return JsonSerializer.Serialize(this, options);
    }
}

public class AffinityProfileConfig
{
    public string Name { get; set; } = string.Empty;
    public string AffinityMask { get; set; } = string.Empty;
}

public class TargetProcessRuleConfig
{
    public string ProcessName { get; set; } = string.Empty;

    public string Profile { get; set; } = string.Empty;
    
    public int DelayDuration { get; set; }
}

public class TargetFolderRuleConfig
{
    public string FolderPath { get; set; } = string.Empty;

    public string Profile { get; set; } = string.Empty;
    
    public int DelayDuration { get; set; }
}