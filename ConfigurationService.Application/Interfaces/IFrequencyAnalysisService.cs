using ConfigurationService.Contracts;

namespace ConfigurationService.Application;

public interface IFrequencyAnalysisService
{
    // Returns candidate FDM band centers and stats needed for downstream steps
    Task<IReadOnlyList<FrequencyBand>> AnalyzeAsync(AnalysisOptions options, CancellationToken ct = default);
}
