using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ConfigSetup.Domain.Scpi;
using Microsoft.Extensions.Logging;

namespace ConfigSetup.Application.Instrumentation;

/// <summary>
/// Streams SCPI commands to a connected instrument through an <see cref="IInstrumentSessionFactory"/>.
/// </summary>
public sealed class InstrumentCommandExecutor : IInstrumentCommandExecutor
{
    private readonly IInstrumentSessionFactory _sessionFactory;
    private readonly ILogger<InstrumentCommandExecutor> _logger;

    public InstrumentCommandExecutor(IInstrumentSessionFactory sessionFactory, ILogger<InstrumentCommandExecutor> logger)
    {
        _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<InstrumentExecutionResult> ExecuteAsync(
        InstrumentExecutionContext context,
        IEnumerable<ScpiCommand> commands,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commands);

        var commandList = commands
            .Where(static command => !string.IsNullOrWhiteSpace(command.Text))
            .ToList();

        if (commandList.Count == 0)
        {
            throw new InvalidOperationException("At least one SCPI command must be provided.");
        }

        await using var session = await _sessionFactory.CreateAsync(context.ResourceAddress, cancellationToken).ConfigureAwait(false);
        var stopwatch = Stopwatch.StartNew();

        foreach (var command in commandList)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await session.SendAsync(command.Text, cancellationToken).ConfigureAwait(false);
        }

        stopwatch.Stop();
        _logger.LogInformation(
            "Streamed {CommandCount} SCPI command(s) to {Resource} in {Duration} ms ({Sequence}).",
            commandList.Count,
            context.ResourceAddress,
            stopwatch.Elapsed.TotalMilliseconds,
            context.SequenceName ?? "ad-hoc batch");

        return new InstrumentExecutionResult(context.ResourceAddress, commandList.Count, stopwatch.Elapsed, context.SequenceName);
    }
}
