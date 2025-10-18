using System.Linq;

using ConfigSetup.Application.Scpi;
using ConfigSetup.Domain.Models;
using Microsoft.Extensions.Logging;

namespace ConfigSetup.Tests.Scpi;

public class ScpiCommandGeneratorTests
{
    private static readonly ILoggerFactory LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(static builder => builder.SetMinimumLevel(LogLevel.None));

    [Fact]
    public void Generate_ReturnsCommandsForDevice()
    {
        var device = new DeviceConfiguration(
            "VXG-C",
            "SOUR1",
            "1.2GHz",
            "-10dBm",
            "FDM",
            new[]
            {
                new DeviceParameter("WAVE:TYPE", "SINE"),
                new DeviceParameter("SOUR2:MOD", "AM")
            });

        var configuration = new HardwareConfiguration(new[] { device });
        var generator = new ScpiCommandGenerator(LoggerFactory.CreateLogger<ScpiCommandGenerator>());

        var commands = generator.Generate(configuration);

        Assert.Contains(commands, c => c.Text == "SOUR1:FREQ:CW 1.2GHz");
        Assert.Contains(commands, c => c.Text == "SOUR1:POW -10dBm");
        Assert.Contains(commands, c => c.Text == "SOUR1:MODE FDM");
        Assert.Contains(commands, c => c.Text == "SOUR1:WAVE:TYPE SINE");
        Assert.Contains(commands, c => c.Text == "SOUR2:MOD AM");
    }

    [Fact]
    public void Generate_UsesDefaultSourceWhenMissing()
    {
        var device = new DeviceConfiguration(
            "VXG-C",
            null,
            "1.2GHz",
            null,
            null,
            new[] { new DeviceParameter("POW", "-10dBm") });

        var configuration = new HardwareConfiguration(new[] { device });
        var generator = new ScpiCommandGenerator(LoggerFactory.CreateLogger<ScpiCommandGenerator>());

        var commands = generator.Generate(configuration);

        Assert.Contains(commands, c => c.Text == "SOUR1:FREQ:CW 1.2GHz");
        Assert.Contains(commands, c => c.Text == "SOUR1:POW -10dBm");
    }
}
