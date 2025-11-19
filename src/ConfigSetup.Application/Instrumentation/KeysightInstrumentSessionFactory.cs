using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConfigSetup.Application.Instrumentation;

/// <summary>
/// Creates VISA sessions using the Keysight IO libraries through the native VISA API.
/// </summary>
public sealed class KeysightInstrumentSessionFactory : IInstrumentSessionFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly InstrumentOptions _options;

    public KeysightInstrumentSessionFactory(ILoggerFactory loggerFactory, IOptions<InstrumentOptions> options)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public Task<IInstrumentSession> CreateAsync(string resourceAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resourceAddress))
        {
            throw new ArgumentException("A resource address is required.", nameof(resourceAddress));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var address = resourceAddress.Trim();
        var managerHandle = VisaNativeMethods.OpenDefaultResourceManager();

        try
        {
            var sessionHandle = VisaNativeMethods.OpenInstrument(managerHandle, address);
            VisaNativeMethods.ConfigureTimeout(sessionHandle, (uint)_options.CommandTimeoutMilliseconds);
            VisaNativeMethods.ConfigureTermination(sessionHandle, enable: true, (byte)'\n');

            var logger = _loggerFactory.CreateLogger<KeysightInstrumentSession>();
            return Task.FromResult<IInstrumentSession>(new KeysightInstrumentSession(managerHandle, sessionHandle, address, logger));
        }
        catch
        {
            VisaNativeMethods.Close(managerHandle);
            throw;
        }
    }
}
