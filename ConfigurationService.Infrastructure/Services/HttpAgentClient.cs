using System.Net.Http.Json;
using ConfigurationService.Application;
using ConfigurationService.Contracts;

namespace ConfigurationService.Infrastructure.Services;

public sealed class HttpAgentClient : IConfigPusher
{
    private readonly HttpClient _http;

    public HttpAgentClient(HttpClient http) => _http = http;

    public async Task<bool> PushAsync(AgentAddress agent, MachineConfig config, CancellationToken ct = default)
    {
        if (!string.Equals(agent.Protocol, "http", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(agent.Protocol, "https", StringComparison.OrdinalIgnoreCase))
            return false; // Not responsible for non-HTTP protocols

        using var req = new HttpRequestMessage(HttpMethod.Post, agent.Endpoint)
        {
            Content = JsonContent.Create(config)
        };

        if (!string.IsNullOrWhiteSpace(agent.ApiKey))
            req.Headers.Add("X-API-Key", agent.ApiKey);

        using var resp = await _http.SendAsync(req, ct);
        return resp.IsSuccessStatusCode;
    }
}