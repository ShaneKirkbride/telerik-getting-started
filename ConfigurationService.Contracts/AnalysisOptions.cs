namespace ConfigurationService.Contracts;

public sealed class AnalysisOptions
{
    // MATLAB inputs
    public double SectionBandwidthMHz { get; set; } = 50;         // sectionBW
    public int PpsFdmAssign { get; set; } = 15000;                // ppsFDMAssign
    public bool FdmPresent { get; set; } = true;                  // FDMPresent
    public bool RandomizeShadowTime { get; set; } = true;         // randomizeShadowTime
    public int BufferTimePs { get; set; } = 20000;                // bufferTime (picoseconds)
    public int NumberOfVirtualChannels { get; set; } = 8;         // numberOfVirtChannels
    public double MaxFdmPowerDb { get; set; } = -5;               // maxFDMPower
    public int MaxChanAvail { get; set; } = 1000;                 // maxChanAvail
    public bool TdmEnabled { get; set; } = false;                 // TDMTrue
    public bool UxgEnabled { get; set; } = true;                  // UXGTrue
    public int RecPortCount { get; set; } = 6;                    // recPortCount
    public double LabWidthFt { get; set; } = 40;                  // Width
    public double LabLengthFt { get; set; } = 100;                // Length
    public double MaxFreqPlayedGhz { get; set; } = 40;            // maxFreqPlayed

    // Future: filenames, NEWEGPDW, etc. omitted for skeleton
}

public sealed record FrequencyBand(double MinMHz, double CenterMHz, double MaxMHz);

public sealed record FdmBandStats(
    int BandNumber,
    double MinMHz,
    double CenterMHz,
    double MaxMHz,
    int VcAvailable,
    int MaxVcUsed,
    long PulsesAssigned,
    long PulsesPlayed,
    long PulsesDropped,
    double PulseDropPercent);

public sealed record FdmAnalysisResult(IReadOnlyList<FdmBandStats> Bands, long PulsesToTdmUxg);

public sealed record TdmUxgAnalysisResult(IReadOnlyList<int> Channels); // simple placeholder

public sealed record PortExpansionCoolingRow(
    int NumOfVxgUnits,
    double VxgCoolingTons,
    int NumOfVuxg,
    double VuxgCoolingTons,
    int NumOfAuxg,
    double AuxgCoolingTons,
    double TotalHwCoolingTons);

public sealed record PortExpansionCoolingSummary(
    double TotalHwCoolingTons,
    double LabSizeSqft,
    double LabCoolingReqTons,
    double TotalOverallCoolingReqTons);

public sealed record PortExpansionResult(
    IReadOnlyList<PortExpansionCoolingRow> HardwareCooling,
    PortExpansionCoolingSummary Summary);
