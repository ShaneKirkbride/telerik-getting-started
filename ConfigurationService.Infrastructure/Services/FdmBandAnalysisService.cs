using ConfigurationService.Application;
using ConfigurationService.Contracts;

namespace ConfigurationService.Infrastructure.Services;

public sealed class FdmBandAnalysisService : IFdmBandAnalysisService
{
    public Task<FdmAnalysisResult> AnalyzeAsync(IReadOnlyList<FrequencyBand> fdmBandCenters, AnalysisOptions options, CancellationToken ct = default)
    {
        var stats = new List<FdmBandStats>();
        int bandNum = 1;
        foreach (var b in fdmBandCenters)
        {
            // Toy numbers: pretend some pulses were assigned and dropped
            long assigned = 100_000;
            long dropped = 2_500;
            long played = assigned - dropped;
            double dropPct = assigned == 0 ? 0 : (double)dropped / assigned * 100.0;
            stats.Add(new FdmBandStats(
                BandNumber: bandNum++,
                MinMHz: b.MinMHz,
                CenterMHz: b.CenterMHz,
                MaxMHz: b.MaxMHz,
                VcAvailable: options.NumberOfVirtualChannels,
                MaxVcUsed: Math.Min(options.NumberOfVirtualChannels, 6),
                PulsesAssigned: assigned,
                PulsesPlayed: played,
                PulsesDropped: dropped,
                PulseDropPercent: dropPct));
        }

        // Pulses going to TDM/UXG (placeholder)
        long pulsesToTdmUxg = 250_000;
        return Task.FromResult(new FdmAnalysisResult(stats, pulsesToTdmUxg));
    }
}
