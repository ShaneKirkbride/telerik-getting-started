using ConfigurationService.Contracts;

namespace ConfigurationService.Application;

public interface IPortExpansionService
{
    Task<PortExpansionResult> ComputeAsync(
        double maxFreqPlayedGhz,
        TdmUxgAnalysisResult tdmUxgResult,
        IReadOnlyList<FrequencyBand> fdmBandCenters,
        AnalysisOptions options,
        CancellationToken ct = default);
}
