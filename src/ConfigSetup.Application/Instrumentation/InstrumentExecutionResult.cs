using System;

namespace ConfigSetup.Application.Instrumentation;

/// <summary>
/// Captures execution metadata returned after streaming commands to an instrument.
/// </summary>
public sealed class InstrumentExecutionResult
{
    public InstrumentExecutionResult(string resourceAddress, int commandCount, TimeSpan duration, string? sequenceName)
    {
        if (string.IsNullOrWhiteSpace(resourceAddress))
        {
            throw new ArgumentException("A resource address must be provided.", nameof(resourceAddress));
        }

        if (commandCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(commandCount));
        }

        ResourceAddress = resourceAddress;
        CommandCount = commandCount;
        Duration = duration < TimeSpan.Zero ? TimeSpan.Zero : duration;
        SequenceName = sequenceName;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public string ResourceAddress { get; }

    public int CommandCount { get; }

    public TimeSpan Duration { get; }

    public string? SequenceName { get; }

    public DateTimeOffset CompletedAt { get; }
}
