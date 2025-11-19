using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ConfigSetup.Domain.Scpi;

namespace ConfigSetup.Application.Instrumentation;

/// <summary>
/// Abstraction for services that can stream SCPI commands to an attached instrument.
/// </summary>
public interface IInstrumentCommandExecutor
{
    Task<InstrumentExecutionResult> ExecuteAsync(
        InstrumentExecutionContext context,
        IEnumerable<ScpiCommand> commands,
        CancellationToken cancellationToken = default);
}
