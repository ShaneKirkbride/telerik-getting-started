using System;
using System.Collections.Generic;

namespace ConfigSetup.Web.Contracts;

/// <summary>
/// Represents the payload required to export an XML configuration document.
/// </summary>
public sealed class ExportConfigurationRequest
{
    public IList<ExportDeviceRequest> Devices { get; init; } = new List<ExportDeviceRequest>();

    public void EnsureIsValid()
    {
        if (Devices.Count == 0)
        {
            throw new InvalidOperationException("At least one device is required to build an export document.");
        }

        foreach (var device in Devices)
        {
            device.EnsureIsValid();
        }
    }
}

/// <summary>
/// Describes a device entry included in the export payload.
/// </summary>
public sealed class ExportDeviceRequest
{
    public string Name { get; init; } = string.Empty;

    public string? Source { get; init; }

    public string? Frequency { get; init; }

    public string? Power { get; init; }

    public string? Mode { get; init; }

    public ExportConnectionRequest? Connection { get; init; }

    public IList<ExportParameterRequest> Parameters { get; init; } = new List<ExportParameterRequest>();

    public void EnsureIsValid()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("Each device must specify a name.");
        }

        foreach (var parameter in Parameters)
        {
            parameter.EnsureIsValid();
        }

        Connection?.EnsureIsValid();
    }
}

/// <summary>
/// Transport type for describing per-device instrument connectivity.
/// </summary>
public sealed class ExportConnectionRequest
{
    public string? Protocol { get; init; }

    public string? Address { get; init; }

    public string? Port { get; init; }

    public void EnsureIsValid()
    {
        if (string.IsNullOrWhiteSpace(Address) && string.IsNullOrWhiteSpace(Protocol) && string.IsNullOrWhiteSpace(Port))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Address))
        {
            throw new InvalidOperationException("When a connection is provided, an address is required.");
        }
    }
}

/// <summary>
/// Describes an additional parameter assigned to a device entry within the export payload.
/// </summary>
public sealed class ExportParameterRequest
{
    public string Name { get; init; } = string.Empty;

    public string? Value { get; init; }

    public void EnsureIsValid()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("Parameter entries must include a name.");
        }
    }
}
