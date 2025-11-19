using System;

namespace ConfigSetup.Domain.Models;

/// <summary>
/// Represents instrument connectivity metadata attached to a device entry.
/// </summary>
public sealed class InstrumentConnection
{
    public InstrumentConnection(string? protocol, string? host, string? port)
    {
        Protocol = Normalize(protocol);
        Host = Normalize(host);
        Port = Normalize(port);
    }

    public string? Protocol { get; }

    public string? Host { get; }

    public string? Port { get; }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
