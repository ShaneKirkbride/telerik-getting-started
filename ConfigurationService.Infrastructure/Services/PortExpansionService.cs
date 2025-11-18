using ConfigurationService.Application;
using ConfigurationService.Contracts;

namespace ConfigurationService.Infrastructure.Services;

public sealed class PortExpansionService : IPortExpansionService
{
    public Task<PortExpansionResult> ComputeAsync(
        double maxFreqPlayedGhz,
        TdmUxgAnalysisResult tdmUxgResult,
        IReadOnlyList<FrequencyBand> fdmBandCenters,
        AnalysisOptions options,
        CancellationToken ct = default)
    {
        // --- MATLAB mapping ---
        // VXGPortPerBox = (maxFreqPlayed > 20 GHz) ? 2 : 4
        int vxgPortPerBox = maxFreqPlayedGhz > 20.0 ? 2 : 4;

        int totFdmBands = options.FdmPresent ? fdmBandCenters.Count : 0;

        // numOfPortsReq = totFDMBands * recPortCount
        int numOfPortsReq = totFdmBands * options.RecPortCount;

        // numOfVXGUnits = numOfPortsReq / VXGPortPerBox (allow non-integer -> round up)
        int numOfVxgUnits = (int)Math.Ceiling(numOfPortsReq / (double)vxgPortPerBox);

        // UXG side: for each channels[n], NumOfUXGReq[n] = channels[n] * recPortCount
        var channels = tdmUxgResult.Channels?.ToList() ?? new List<int>();
        var numOfUxgReq = channels.Select(ch => ch * options.RecPortCount).ToList();
        var totalNumAnalog = channels; // Following MATLAB naming

        // --- Cooling calculations ---
        // Constants per device (BTU numbers from your script):
        // VXG: 1500 W * 3.41 BTU/W
        // V-UXG: 800 W * 3.41
        // A-UXG: 600 W * 3.41
        double VXG_BTU_PER_UNIT = 1500 * 3.41;
        double VUXG_BTU_PER_UNIT = 800 * 3.41;
        double AUXG_BTU_PER_UNIT = 600 * 3.41;

        // Per-row hardware totals
        // Rows correspond to each channel scenario (the MATLAB flips for display; here we keep input order)
        var rows = new List<PortExpansionCoolingRow>();
        for (int i = 0; i < channels.Count; i++)
        {
            int numVuxg = numOfUxgReq[i];
            int numAuxg = totalNumAnalog[i];

            double vxgBTU = numOfVxgUnits * VXG_BTU_PER_UNIT;
            double vuxgBTU = numVuxg * VUXG_BTU_PER_UNIT;
            double auxgBTU = numAuxg * AUXG_BTU_PER_UNIT;

            double hardwareBTU = vxgBTU + vuxgBTU + auxgBTU;
            double hardwareTons = BtuToTons(hardwareBTU);
            double vxgTons = BtuToTons(vxgBTU);
            double vuxgTons = BtuToTons(vuxgBTU);
            double auxgTons = BtuToTons(auxgBTU);

            rows.Add(new PortExpansionCoolingRow(
                NumOfVxgUnits: numOfVxgUnits,
                VxgCoolingTons: Round2(vxgTons),
                NumOfVuxg: numVuxg,
                VuxgCoolingTons: Round2(vuxgTons),
                NumOfAuxg: numAuxg,
                AuxgCoolingTons: Round2(auxgTons),
                TotalHwCoolingTons: Round2(hardwareTons)
            ));
        }

        // Lab cooling: labBTU = (squareFootage * 20)
        double labSqft = options.LabWidthFt * options.LabLengthFt;
        double labBTU = labSqft * 20.0;
        double labTons = BtuToTons(labBTU);

        // Summary using last hardware row or 0 if none
        double totalHwTons = rows.LastOrDefault()?.TotalHwCoolingTons ?? 0.0;
        double totalOverallTons = Round2(totalHwTons + labTons);

        var summary = new PortExpansionCoolingSummary(
            TotalHwCoolingTons: Round2(totalHwTons),
            LabSizeSqft: Round2(labSqft),
            LabCoolingReqTons: Round2(labTons),
            TotalOverallCoolingReqTons: totalOverallTons
        );

        return Task.FromResult(new PortExpansionResult(rows, summary));

        static double BtuToTons(double btu) => btu * 8.33333e-5;
        static double Round2(double v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
