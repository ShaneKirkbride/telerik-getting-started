using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace ConfigSetup.Application.Instrumentation;

internal sealed class KeysightInstrumentSession : IInstrumentSession
{
    private readonly int _resourceManagerHandle;
    private readonly int _sessionHandle;
    private readonly ILogger<KeysightInstrumentSession> _logger;

    public KeysightInstrumentSession(int resourceManagerHandle, int sessionHandle, string resourceAddress, ILogger<KeysightInstrumentSession> logger)
    {
        if (string.IsNullOrWhiteSpace(resourceAddress))
        {
            throw new ArgumentException("A resource address is required.", nameof(resourceAddress));
        }

        _resourceManagerHandle = resourceManagerHandle;
        _sessionHandle = sessionHandle;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ResourceAddress = resourceAddress;
    }

    public string ResourceAddress { get; }

    public Task SendAsync(string command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return Task.CompletedTask;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var payload = Encoding.ASCII.GetBytes(command.TrimEnd('\r', '\n') + "\n");
        VisaNativeMethods.Write(_sessionHandle, payload);
        _logger.LogDebug("SCPI => {Command}", command.Trim());
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        VisaNativeMethods.Close(_sessionHandle);
        VisaNativeMethods.Close(_resourceManagerHandle);
        return ValueTask.CompletedTask;
    }
}
