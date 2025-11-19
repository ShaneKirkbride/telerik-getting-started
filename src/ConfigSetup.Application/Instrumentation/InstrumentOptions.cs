namespace ConfigSetup.Application.Instrumentation;

/// <summary>
/// Provides configuration for instrument interactions handled through the Keysight IO Libraries stack.
/// </summary>
public sealed class InstrumentOptions
{
    public const string SectionName = "Instrument";

    /// <summary>
    /// Gets or sets the default VISA resource address that the UI should pre-populate.
    /// </summary>
    public string DefaultResourceAddress { get; set; } = "TCPIP0::127.0.0.1::inst0::INSTR";

    /// <summary>
    /// Gets or sets the timeout (in milliseconds) applied to VISA sessions.
    /// </summary>
    public int CommandTimeoutMilliseconds { get; set; } = 3000;
}
