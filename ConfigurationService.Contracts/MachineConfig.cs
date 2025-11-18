namespace ConfigurationService.Contracts;

public sealed record AgentAddress(
    string Id,
    string Protocol,   // "http", "grpc", or custom
    string Endpoint,   // URL (http(s)) or authority for gRPC
    string? ApiKey);

public sealed record MachineIdentity(string Name, string Location, string Version);

public sealed record MachineNetwork(IReadOnlyList<AgentAddress> Agents);

public sealed record FdmConfig(bool Present, int NumberOfVirtualChannels, double MaxPowerDb);

public sealed record LabConfig(double WidthFt, double LengthFt);

public sealed record PlaybackConfig(double MaxFreqPlayedGhz, int RecPortCount, bool RandomizeShadowTime, int BufferTimePs);

public sealed record MachineFrequencyBand(double MinMHz, double CenterMHz, double MaxMHz);

public sealed record MachineConfig(
    MachineIdentity Identity,
    MachineNetwork Network,
    FdmConfig Fdm,
    LabConfig Lab,
    PlaybackConfig Playback,
    IReadOnlyList<MachineFrequencyBand> Bands);
