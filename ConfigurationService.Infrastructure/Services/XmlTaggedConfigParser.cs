using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using ConfigurationService.Application;
using ConfigurationService.Contracts;

namespace ConfigurationService.Infrastructure.Services;

public sealed class XmlTaggedConfigParser : ITaggedConfigParser
{
    public async Task<MachineConfig> ParseAsync(Stream xmlStream, Stream? xsdStream = null, CancellationToken ct = default)
    {
        // Load XML
        using var xmlReader = XmlReader.Create(xmlStream, new XmlReaderSettings { Async = true });
        var xdoc = await Task.Run(() => XDocument.Load(xmlReader, LoadOptions.None), ct);

        // Optional XSD validation
        if (xsdStream is not null)
        {
            var schemas = new XmlSchemaSet();
            schemas.Add(null, XmlReader.Create(xsdStream));
            var errors = new List<string>();
            xdoc.Validate(schemas, (o, e) => errors.Add(e.Message));
            if (errors.Count > 0)
                throw new InvalidOperationException("XML validation failed: " + string.Join("; ", errors));
        }

        // Map to DTOs
        var root = xdoc.Element("MachineConfig") ?? throw new InvalidOperationException("Missing root <MachineConfig>.");

        var idElem = root.Element("Identity") ?? throw new InvalidOperationException("Missing <Identity>.");
        var identity = new MachineIdentity(
            idElem.Attribute("Name")?.Value ?? throw new("Identity.Name missing"),
            idElem.Attribute("Location")?.Value ?? throw new("Identity.Location missing"),
            idElem.Attribute("Version")?.Value ?? throw new("Identity.Version missing"));

        var netElem = root.Element("Network") ?? throw new InvalidOperationException("Missing <Network>.");
        var agents = netElem.Elements("Agent")
            .Select(a => new AgentAddress(
                a.Attribute("Id")?.Value ?? throw new("Agent.Id missing"),
                a.Attribute("Protocol")?.Value ?? "http",
                a.Attribute("Endpoint")?.Value ?? throw new("Agent.Endpoint missing"),
                a.Attribute("ApiKey")?.Value))
            .ToList()
            .AsReadOnly();
        var network = new MachineNetwork(agents);

        var fdmElem = root.Element("Fdm") ?? throw new InvalidOperationException("Missing <Fdm>.");
        var fdm = new FdmConfig(
            bool.Parse(fdmElem.Attribute("Present")?.Value ?? "false"),
            int.Parse(fdmElem.Attribute("NumberOfVirtualChannels")?.Value ?? "0"),
            double.Parse(fdmElem.Attribute("MaxPowerDb")?.Value ?? "-1000"));

        var labElem = root.Element("Lab") ?? throw new InvalidOperationException("Missing <Lab>.");
        var lab = new LabConfig(
            double.Parse(labElem.Attribute("WidthFt")?.Value ?? "0"),
            double.Parse(labElem.Attribute("LengthFt")?.Value ?? "0"));

        var pbElem = root.Element("Playback") ?? throw new InvalidOperationException("Missing <Playback>.");
        var playback = new PlaybackConfig(
            double.Parse(pbElem.Attribute("MaxFreqPlayedGhz")?.Value ?? "0"),
            int.Parse(pbElem.Attribute("RecPortCount")?.Value ?? "0"),
            bool.Parse(pbElem.Attribute("RandomizeShadowTime")?.Value ?? "false"),
            int.Parse(pbElem.Attribute("BufferTimePs")?.Value ?? "0"));

        var bandsElem = root.Element("Bands") ?? throw new InvalidOperationException("Missing <Bands>.");
        var bands = bandsElem.Elements("Band")
            .Select(b => new MachineFrequencyBand(
                double.Parse(b.Attribute("MinMHz")?.Value ?? "0"),
                double.Parse(b.Attribute("CenterMHz")?.Value ?? "0"),
                double.Parse(b.Attribute("MaxMHz")?.Value ?? "0")))
            .ToList()
            .AsReadOnly();

        return new MachineConfig(identity, network, fdm, lab, playback, bands);
    }
}
