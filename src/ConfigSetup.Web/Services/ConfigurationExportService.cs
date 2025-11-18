using System;
using System.Linq;
using System.Xml.Linq;

using ConfigSetup.Web.Contracts;

namespace ConfigSetup.Web.Services;

/// <summary>
/// Builds XML export documents from client supplied configuration requests.
/// </summary>
public sealed class ConfigurationExportService
{
    public XDocument CreateDocument(ExportConfigurationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.EnsureIsValid();

        var configuration = new XElement("Configuration",
            request.Devices.Select(device => BuildDeviceElement(device)));

        return new XDocument(new XDeclaration("1.0", "utf-8", "yes"), configuration);
    }

    private static XElement BuildDeviceElement(ExportDeviceRequest device)
    {
        var element = new XElement("Device", new XAttribute("name", device.Name.Trim()));

        var sourceValue = Normalize(device.Source);
        if (!string.IsNullOrWhiteSpace(sourceValue))
        {
            element.SetAttributeValue("source", sourceValue);
        }

        AppendChildElement(element, "Frequency", device.Frequency);
        AppendChildElement(element, "Power", device.Power);
        AppendChildElement(element, "Mode", device.Mode);

        foreach (var parameter in device.Parameters)
        {
            var value = Normalize(parameter.Value);
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            element.Add(new XElement(
                "Parameter",
                new XAttribute("name", parameter.Name.Trim()),
                new XAttribute("value", value)));
        }

        return element;
    }

    private static void AppendChildElement(XContainer container, string name, string? value)
    {
        var content = Normalize(value);
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        container.Add(new XElement(name, content));
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
