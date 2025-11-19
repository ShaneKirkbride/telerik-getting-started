using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigSetup.Application.Instrumentation;

/// <summary>
/// Represents an active connection to an instrument.
/// </summary>
public interface IInstrumentSession : IAsyncDisposable
{
    string ResourceAddress { get; }

    Task SendAsync(string command, CancellationToken cancellationToken = default);
}
