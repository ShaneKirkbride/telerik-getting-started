using ConfigurationService.Application;
using ConfigurationService.Contracts;

namespace ConfigurationService.Infrastructure.Services;

public sealed class FrequencyAnalysisService : IFrequencyAnalysisService
{
    public Task<IReadOnlyList<FrequencyBand>> AnalyzeAsync(AnalysisOptions options, CancellationToken ct = default)
    {
        // Placeholder: in real impl, parse data and build bands using section bandwidth & power rules
        var min = 500.0;         // MHz
        var center = min + 1250; // MHz
        var max = min + 2500;    // MHz
        IReadOnlyList<FrequencyBand> bands = new[] { new FrequencyBand(min, center, max) };
        return Task.FromResult(bands);
    }
}
