using System.Threading;
using System.Threading.Tasks;

namespace ConfigSetup.Application.Instrumentation;

/// <summary>
/// Factory responsible for creating instrument sessions backed by Keysight IO Libraries.
/// </summary>
public interface IInstrumentSessionFactory
{
    Task<IInstrumentSession> CreateAsync(string resourceAddress, CancellationToken cancellationToken = default);
}
