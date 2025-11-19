using System;
using System.Xml.Linq;

using ConfigSetup.Domain.Models;

namespace ConfigSetup.Web.ViewModels;

/// <summary>
/// Represents per-device connectivity details surfaced in the UI.
/// </summary>
public sealed class InstrumentConnectionEditorViewModel
{
    public InstrumentConnectionEditorViewModel(string? protocol = null, string? host = null, string? port = null)
    {
        DefaultProtocol = Normalize(protocol) ?? "HiSLIP";
        DefaultHost = Normalize(host) ?? string.Empty;
        DefaultPort = Normalize(port) ?? "4880";

        Protocol = DefaultProtocol;
        Host = DefaultHost;
        Port = DefaultPort;
    }

    public string DefaultProtocol { get; }

    public string DefaultHost { get; }

    public string DefaultPort { get; }

    public string Protocol { get; set; }

    public string Host { get; set; }

    public string Port { get; set; }

    public void ResetToDefault()
    {
        Protocol = DefaultProtocol;
        Host = DefaultHost;
        Port = DefaultPort;
    }

    public void Apply(InstrumentConnection? connection)
    {
        if (connection is null)
        {
            ResetToDefault();
            return;
        }

        Protocol = Normalize(connection.Protocol) ?? DefaultProtocol;
        Host = Normalize(connection.Host) ?? DefaultHost;
        Port = Normalize(connection.Port) ?? DefaultPort;
    }

    public XElement? ToXElement()
    {
        var address = Normalize(Host);
        var hasAddress = !string.IsNullOrWhiteSpace(address);
        var protocol = hasAddress ? Normalize(Protocol, DefaultProtocol) : Normalize(Protocol);
        var port = hasAddress ? Normalize(Port, DefaultPort) : Normalize(Port);

        if (!hasAddress)
        {
            return null;
        }

        var element = new XElement("Connection");
        if (!string.IsNullOrWhiteSpace(protocol))
        {
            element.SetAttributeValue("protocol", protocol);
        }

        if (!string.IsNullOrWhiteSpace(address))
        {
            element.Add(new XElement("Address", address));
        }

        if (!string.IsNullOrWhiteSpace(port))
        {
            element.Add(new XElement("Port", port));
        }

        return element;
    }

    public InstrumentConnection ToConnectionModel()
    {
        return new InstrumentConnection(
            Normalize(Protocol, DefaultProtocol),
            Normalize(Host),
            Normalize(Port, DefaultPort));
    }

    public string BuildVisaResourceAddress()
    {
        var address = Normalize(Host);
        if (string.IsNullOrWhiteSpace(address))
        {
            return string.Empty;
        }

        var protocolSegment = string.Equals(Normalize(Protocol, DefaultProtocol), "HISLIP", StringComparison.OrdinalIgnoreCase)
            ? "hislip0"
            : "inst0";

        var portSegment = Normalize(Port, DefaultPort);
        var portSuffix = string.IsNullOrWhiteSpace(portSegment) ? string.Empty : $"::{portSegment}";

        return $"TCPIP0::{address}::{protocolSegment}{portSuffix}::INSTR";
    }

    private static string? Normalize(string? value, string? fallback = null)
    {
        var result = string.IsNullOrWhiteSpace(value) ? fallback : value?.Trim();
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }
}
