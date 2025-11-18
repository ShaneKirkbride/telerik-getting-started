using ConfigurationService.Application;
using ConfigurationService.Contracts;

namespace ConfigurationService.Infrastructure.Services;

public sealed class ConfigurationOrchestrator : IConfigurationOrchestrator
{
    private readonly IFrequencyAnalysisService _freq;
    private readonly IFdmBandAnalysisService _fdm;
    private readonly ITdmUxgAnalysisService _tdm;
    private readonly IPortExpansionService _ports;

    public ConfigurationOrchestrator(
        IFrequencyAnalysisService freq,
        IFdmBandAnalysisService fdm,
        ITdmUxgAnalysisService tdm,
        IPortExpansionService ports)
    {
        _freq = freq;
        _fdm = fdm;
        _tdm = tdm;
        _ports = ports;
    }

    public async Task<(FdmAnalysisResult fdm, TdmUxgAnalysisResult tdm, PortExpansionResult ports)> RunAsync(
        AnalysisOptions options,
        CancellationToken ct = default)
    {
        var bands = await _freq.AnalyzeAsync(options, ct);
        var fdmResult = await _fdm.AnalyzeAsync(bands, options, ct);
        var tdmResult = await _tdm.AnalyzeAsync(fdmResult, options, ct);
        var portResult = await _ports.ComputeAsync(options.MaxFreqPlayedGhz, tdmResult, bands, options, ct);
        return (fdmResult, tdmResult, portResult);
    }
}
