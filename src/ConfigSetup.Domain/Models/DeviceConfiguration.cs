using System;
using System.Collections.Generic;
using System.Linq;

using System.Collections.ObjectModel;

namespace ConfigSetup.Domain.Models;

/// <summary>
/// Describes a single device entry captured from an input configuration document.
/// </summary>
public sealed class DeviceConfiguration
{
    private readonly IReadOnlyList<DeviceParameter> _parameters;

    public DeviceConfiguration(
        string name,
        string? source,
        string? frequency,
        string? power,
        string? mode,
        IEnumerable<DeviceParameter>? parameters = null,
        InstrumentConnection? connection = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim();
        Frequency = string.IsNullOrWhiteSpace(frequency) ? null : frequency.Trim();
        Power = string.IsNullOrWhiteSpace(power) ? null : power.Trim();
        Mode = string.IsNullOrWhiteSpace(mode) ? null : mode.Trim();
        _parameters = new ReadOnlyCollection<DeviceParameter>((parameters ?? Array.Empty<DeviceParameter>()).ToArray());
        Connection = connection;
    }

    /// <summary>
    /// Friendly name of the device, typically matching the configuration document.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Optional SCPI source prefix (for example, <c>SOUR1</c>).
    /// </summary>
    public string? Source { get; }

    public string? Frequency { get; }

    public string? Power { get; }

    public string? Mode { get; }

    public InstrumentConnection? Connection { get; }

    /// <summary>
    /// Additional named parameters that may appear in the document.
    /// </summary>
    public IReadOnlyList<DeviceParameter> Parameters => _parameters;
}
