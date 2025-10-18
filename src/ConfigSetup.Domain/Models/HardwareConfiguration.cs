using System;
using System.Collections.Generic;
using System.Linq;

using System.Collections.ObjectModel;

namespace ConfigSetup.Domain.Models;

/// <summary>
/// Represents the root configuration document.
/// </summary>
public sealed class HardwareConfiguration
{
    private readonly IReadOnlyList<DeviceConfiguration> _devices;

    public HardwareConfiguration(IEnumerable<DeviceConfiguration> devices)
    {
        var deviceList = (devices ?? throw new ArgumentNullException(nameof(devices))).ToArray();
        if (deviceList.Length == 0)
        {
            throw new ArgumentException("The configuration must contain at least one device.", nameof(devices));
        }

        _devices = new ReadOnlyCollection<DeviceConfiguration>(deviceList);
    }

    public IReadOnlyList<DeviceConfiguration> Devices => _devices;
}
