using ConfigSetup.Domain.Models;

namespace ConfigSetup.Application.Configuration;

/// <summary>
/// Parses configuration XML into strongly typed domain models.
/// </summary>
public interface IXmlConfigurationParser
{
    HardwareConfiguration Parse(string xmlContent);
}
