using ConfigSetup.Domain.Models;
using ConfigSetup.Domain.Scpi;

namespace ConfigSetup.Application.Scpi;

/// <summary>
/// Generates SCPI commands from a parsed configuration.
/// </summary>
public interface IScpiCommandGenerator
{
    IReadOnlyList<ScpiCommand> Generate(HardwareConfiguration configuration);
}
