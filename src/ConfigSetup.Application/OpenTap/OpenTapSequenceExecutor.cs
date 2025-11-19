using System;
using System.Threading;
using System.Threading.Tasks;

using ConfigSetup.Application.Instrumentation;

namespace ConfigSetup.Application.OpenTap;

public sealed class OpenTapSequenceExecutor : IOpenTapSequenceExecutor
{
    private readonly IOpenTapSequenceParser _parser;
    private readonly IInstrumentCommandExecutor _instrumentExecutor;

    public OpenTapSequenceExecutor(IOpenTapSequenceParser parser, IInstrumentCommandExecutor instrumentExecutor)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _instrumentExecutor = instrumentExecutor ?? throw new ArgumentNullException(nameof(instrumentExecutor));
    }

    public Task<InstrumentExecutionResult> ExecuteAsync(string content, string fallbackResourceAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fallbackResourceAddress))
        {
            throw new ArgumentException("A fallback VISA resource address must be provided.", nameof(fallbackResourceAddress));
        }

        var sequence = _parser.Parse(content);
        var resourceAddress = string.IsNullOrWhiteSpace(sequence.ResourceAddress)
            ? fallbackResourceAddress
            : sequence.ResourceAddress;

        var context = new InstrumentExecutionContext(resourceAddress!, sequence.Name);
        return _instrumentExecutor.ExecuteAsync(context, sequence.Commands, cancellationToken);
    }
}
