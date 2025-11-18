using ConfigurationService.Contracts;

namespace ConfigurationService.Application;

public interface IConfigPusher
{
    /// <summary>Push a machine config to a specified agent. Return 'true' if accepted.</summary>
    Task<bool> PushAsync(AgentAddress agent, MachineConfig config, CancellationToken ct = default);
}
