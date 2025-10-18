using System;
using System.Collections.Generic;

using ConfigSetup.Domain.Models;
using ConfigSetup.Domain.Scpi;
using Microsoft.Extensions.Logging;

namespace ConfigSetup.Application.Scpi;

/// <summary>
/// Generates a sequential SCPI command list for a hardware configuration.
/// </summary>
public sealed class ScpiCommandGenerator : IScpiCommandGenerator
{
    private readonly ILogger<ScpiCommandGenerator> _logger;

    public ScpiCommandGenerator(ILogger<ScpiCommandGenerator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IReadOnlyList<ScpiCommand> Generate(HardwareConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var commands = new List<ScpiCommand>();
        foreach (var device in configuration.Devices)
        {
            var source = string.IsNullOrWhiteSpace(device.Source) ? "SOUR1" : device.Source!.Trim();
            AppendIfPresent(commands, source, "FREQ:CW", device.Frequency);
            AppendIfPresent(commands, source, "POW", device.Power);
            AppendIfPresent(commands, source, "MODE", device.Mode);

            foreach (var parameter in device.Parameters)
            {
                var name = parameter.Name?.Trim();
                var value = parameter.Value?.Trim();
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                var commandText = name.Contains(':', StringComparison.Ordinal)
                    ? $"{name} {value}".Trim()
                    : $"{source}:{name} {value}".Trim();

                commands.Add(new ScpiCommand(commandText));
            }
        }

        _logger.LogInformation("Generated {Count} SCPI commands.", commands.Count);
        return commands;
    }

    private static void AppendIfPresent(ICollection<ScpiCommand> commands, string source, string command, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        commands.Add(new ScpiCommand($"{source}:{command} {value}".Trim()));
    }
}
