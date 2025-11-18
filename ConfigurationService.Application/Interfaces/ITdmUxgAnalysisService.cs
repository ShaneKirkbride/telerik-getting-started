using ConfigurationService.Contracts;

namespace ConfigurationService.Application;

public interface ITdmUxgAnalysisService
{
    Task<TdmUxgAnalysisResult> AnalyzeAsync(
        FdmAnalysisResult fdmResult,
        AnalysisOptions options,
        CancellationToken ct = default);
}
