using ConfigurationService.Contracts;

namespace ConfigurationService.Application;

public interface IConfigDistributor
{
    /// <summary>Push the config to all (or filtered) agents; returns a per-agent result map.</summary>
    Task<IReadOnlyDictionary<string, bool>> DistributeAsync(MachineConfig config, Func<AgentAddress, bool>? filter = null, CancellationToken ct = default);
}
