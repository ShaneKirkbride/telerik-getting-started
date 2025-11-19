using System;

using ConfigSetup.Application.Configuration;
using ConfigSetup.Domain.Schemas;
using Microsoft.Extensions.Logging;

namespace ConfigSetup.Tests.Configuration;

public class XmlConfigurationParserTests
{
    private static readonly ILoggerFactory LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(static builder => builder.SetMinimumLevel(LogLevel.None));

    private static XmlConfigurationParser CreateParser()
    {
        var schemaSet = ConfigurationSchemaProvider.CreateSchemaSet();
        var parserLogger = LoggerFactory.CreateLogger<XmlConfigurationParser>();
        return new XmlConfigurationParser(schemaSet, parserLogger);
    }

    [Fact]
    public void Parse_WithValidXml_ReturnsConfiguration()
    {
        const string xml = """
<Configuration>
  <Device name="VXG-C" source="SOUR1">
    <Frequency>1.2GHz</Frequency>
    <Power>-10dBm</Power>
    <Mode>FDM</Mode>
    <Connection protocol="HiSLIP">
      <Address>192.168.0.20</Address>
      <Port>4880</Port>
    </Connection>
    <Parameter name="WAVE:TYPE" value="SINE" />
  </Device>
</Configuration>
""";

        var parser = CreateParser();

        var configuration = parser.Parse(xml);

        Assert.Single(configuration.Devices);
        var device = configuration.Devices[0];
        Assert.Equal("VXG-C", device.Name);
        Assert.Equal("SOUR1", device.Source);
        Assert.Equal("1.2GHz", device.Frequency);
        Assert.Equal("-10dBm", device.Power);
        Assert.Equal("FDM", device.Mode);
        Assert.NotNull(device.Connection);
        Assert.Equal("HiSLIP", device.Connection!.Protocol);
        Assert.Equal("192.168.0.20", device.Connection.Host);
        Assert.Equal("4880", device.Connection.Port);
        Assert.Single(device.Parameters);
        Assert.Equal("WAVE:TYPE", device.Parameters[0].Name);
        Assert.Equal("SINE", device.Parameters[0].Value);
    }

    [Fact]
    public void Parse_WithSchemaViolation_Throws()
    {
        const string invalidXml = """
<Configuration>
  <Device>
    <Frequency>1.2GHz</Frequency>
  </Device>
</Configuration>
""";

        var parser = CreateParser();

        var exception = Assert.Throws<InvalidOperationException>(() => parser.Parse(invalidXml));
        Assert.Contains("validation", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
