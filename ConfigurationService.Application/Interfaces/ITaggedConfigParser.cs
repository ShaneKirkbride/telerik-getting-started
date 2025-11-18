using ConfigurationService.Contracts;

namespace ConfigurationService.Application;

public interface ITaggedConfigParser
{
    /// <summary>Parse and validate a tagged (XML) machine config into a DTO.</summary>
    Task<MachineConfig> ParseAsync(Stream xmlStream, Stream? xsdStream = null, CancellationToken ct = default);
}