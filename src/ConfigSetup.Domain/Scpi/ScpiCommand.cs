namespace ConfigSetup.Domain.Scpi;

/// <summary>
/// Represents a single SCPI command to be sent to an instrument.
/// </summary>
/// <param name="Text">The command text.</param>
public readonly record struct ScpiCommand(string Text)
{
    public override string ToString() => Text;
}
