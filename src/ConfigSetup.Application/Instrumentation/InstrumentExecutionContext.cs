using System;

namespace ConfigSetup.Application.Instrumentation;

/// <summary>
/// Provides contextual information describing how commands should be dispatched to an instrument.
/// </summary>
public sealed class InstrumentExecutionContext
{
    public InstrumentExecutionContext(string resourceAddress, string? sequenceName = null)
    {
        if (string.IsNullOrWhiteSpace(resourceAddress))
        {
            throw new ArgumentException("A VISA resource address is required.", nameof(resourceAddress));
        }

        ResourceAddress = resourceAddress.Trim();
        SequenceName = string.IsNullOrWhiteSpace(sequenceName) ? null : sequenceName.Trim();
    }

    /// <summary>
    /// Gets the VISA resource address.
    /// </summary>
    public string ResourceAddress { get; }

    /// <summary>
    /// Gets the optional sequence or batch name used for logging.
    /// </summary>
    public string? SequenceName { get; }
}
