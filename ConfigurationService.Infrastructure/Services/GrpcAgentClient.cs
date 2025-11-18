// using Grpc.Net.Client; // add package if you choose to implement
using ConfigurationService.Application;
using ConfigurationService.Contracts;

namespace ConfigurationService.Infrastructure.Services;

public sealed class GrpcAgentClient : IConfigPusher
{
    public Task<bool> PushAsync(AgentAddress agent, MachineConfig config, CancellationToken ct = default)
    {
        if (!string.Equals(agent.Protocol, "grpc", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(false);

        // TODO: Use GrpcChannel.ForAddress(agent.Endpoint) and call your proto-defined method.
        // Return true on success.
        return Task.FromResult(false);
    }
}