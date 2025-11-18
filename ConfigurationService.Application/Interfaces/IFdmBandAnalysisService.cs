using ConfigurationService.Contracts;

namespace ConfigurationService.Application;

public interface IFdmBandAnalysisService
{
    Task<FdmAnalysisResult> AnalyzeAsync(
        IReadOnlyList<FrequencyBand> fdmBandCenters,
        AnalysisOptions options,
        CancellationToken ct = default);
}
