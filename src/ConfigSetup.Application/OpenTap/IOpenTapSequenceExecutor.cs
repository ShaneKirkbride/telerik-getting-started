using System.Threading;
using System.Threading.Tasks;

using ConfigSetup.Application.Instrumentation;

namespace ConfigSetup.Application.OpenTap;

public interface IOpenTapSequenceExecutor
{
    Task<InstrumentExecutionResult> ExecuteAsync(string content, string fallbackResourceAddress, CancellationToken cancellationToken = default);
}
