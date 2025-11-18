namespace ConfigSetup.Web.Hosting;

/// <summary>
///     Declarative knobs that allow the host to run as a desktop executable or as a background service
///     without resorting to MAUI or platform-specific shims.
/// </summary>
public sealed record ServiceHostOptions
{
    public const string SectionName = "ServiceHost";

    /// <summary>
    ///     Opt-in Windows service integration. Automatically ignored on non-Windows platforms.
    /// </summary>
    public bool EnableWindowsServiceIntegration { get; init; } = true;

    /// <summary>
    ///     Opt-in systemd integration for Linux deployments.
    /// </summary>
    public bool EnableSystemdIntegration { get; init; } = true;

    /// <summary>
    ///     Optional HTTP port override for service scenarios.
    /// </summary>
    public int? HttpPort { get; init; }

    /// <summary>
    ///     Optional HTTPS port override. When omitted, the default ASP.NET Core behavior is preserved.
    /// </summary>
    public int? HttpsPort { get; init; }

    /// <summary>
    ///     Toggle HTTP -> HTTPS redirects when the app is fronted by another reverse proxy.
    /// </summary>
    public bool EnableHttpsRedirection { get; init; } = true;
}
