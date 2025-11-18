using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ConfigSetup.Web.Contracts;

using Microsoft.Extensions.Logging;

namespace ConfigSetup.Web.Services;

/// <summary>
/// Default upload service used during development. The implementation mimics latency and logging to keep
/// the UI responsive without requiring real hardware.
/// </summary>
public sealed class SimulatedSourceUploadService : ISourceUploadService
{
    private static readonly TimeSpan MinimumLatency = TimeSpan.FromMilliseconds(150);

    private readonly ILogger<SimulatedSourceUploadService> _logger;

    public SimulatedSourceUploadService(ILogger<SimulatedSourceUploadService> logger)
    {
        _logger = logger;
    }

    public Task<SourceUploadResult> UploadAsync(SourceUploadRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return UploadAsync(new[] { request }, cancellationToken);
    }

    public async Task<SourceUploadResult> UploadAsync(IEnumerable<SourceUploadRequest> requests, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requests);

        var requestList = requests.Where(static request => request is not null).ToList();
        if (requestList.Count == 0)
        {
            return SourceUploadResult.Empty;
        }

        foreach (var request in requestList)
        {
            request.EnsureIsValid();
        }

        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(MinimumLatency, cancellationToken);

        foreach (var request in requestList)
        {
            _logger.LogInformation(
                "Uploading settings for {SourceName} (Mode={Mode}, Frequency={Frequency}, Power={Power}) with {ParameterCount} parameters.",
                request.Name,
                request.Mode ?? "N/A",
                request.Frequency ?? "N/A",
                request.Power ?? "N/A",
                request.Parameters.Count);
        }

        stopwatch.Stop();
        var totalParameters = requestList.Sum(request => request.Parameters.Count);
        return new SourceUploadResult(requestList.Count, totalParameters, stopwatch.Elapsed);
    }
}
