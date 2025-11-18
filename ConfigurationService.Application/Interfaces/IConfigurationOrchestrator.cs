using ConfigurationService.Contracts;

namespace ConfigurationService.Application;

public interface IConfigurationOrchestrator
{
    Task<(FdmAnalysisResult fdm, TdmUxgAnalysisResult tdm, PortExpansionResult ports)> RunAsync(AnalysisOptions options, CancellationToken ct = default);
}
