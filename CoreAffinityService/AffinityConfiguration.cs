using System.Text.Json;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace CoreAffinityService;

public class AffinityConfiguration
{
    public AffinityProfileConfig[] AffinityProfiles { get; set; } = Array.Empty<AffinityProfileConfig>();
    public ProcessAffinityConfig[] ProcessAffinities { get; set; } = Array.Empty<ProcessAffinityConfig>();
    public FolderAffinityConfig[] FolderAffinities { get; set; } = Array.Empty<FolderAffinityConfig>();

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

public class ProcessAffinityConfig
{
    public string ProcessName { get; set; } = string.Empty;

    public string Profile { get; set; } = string.Empty;
    
    public int DelayDuration { get; set; }
}

public class FolderAffinityConfig
{
    public string FolderPath { get; set; } = string.Empty;

    public string Profile { get; set; } = string.Empty;
    
    public int DelayDuration { get; set; }
}