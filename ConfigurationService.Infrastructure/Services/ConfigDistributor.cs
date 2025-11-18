using ConfigurationService.Application;
using ConfigurationService.Contracts;

namespace ConfigurationService.Infrastructure.Services;

public sealed class ConfigDistributor : IConfigDistributor
{
    private readonly IEnumerable<IConfigPusher> _pushers;

    public ConfigDistributor(IEnumerable<IConfigPusher> pushers)
        => _pushers = pushers;

    public async Task<IReadOnlyDictionary<string, bool>> DistributeAsync(
        MachineConfig config,
        Func<AgentAddress, bool>? filter = null,
        CancellationToken ct = default)
    {
        var results = new Dictionary<string, bool>();
        foreach (var agent in config.Network.Agents.Where(a => filter?.Invoke(a) ?? true))
        {
            bool pushed = false;
            foreach (var pusher in _pushers)
            {
                if (await TryPush(pusher, agent, config, ct))
                {
                    pushed = true;
                    break;
                }
            }
            results[agent.Id] = pushed;
        }
        return results;
    }

    private static async Task<bool> TryPush(IConfigPusher pusher, AgentAddress agent, MachineConfig config, CancellationToken ct)
    {
        try { return await pusher.PushAsync(agent, config, ct); }
        catch { return false; }
    }
}
