using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

using ConfigSetup.Domain.Models;
using Microsoft.Extensions.Logging;

namespace ConfigSetup.Application.Configuration;

/// <summary>
/// Parses configuration XML into strongly typed domain models while validating against the schema.
/// </summary>
public sealed class XmlConfigurationParser : IXmlConfigurationParser
{
    private readonly XmlSchemaSet _schemaSet;
    private readonly ILogger<XmlConfigurationParser> _logger;

    public XmlConfigurationParser(XmlSchemaSet schemaSet, ILogger<XmlConfigurationParser> logger)
    {
        _schemaSet = schemaSet ?? throw new ArgumentNullException(nameof(schemaSet));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public HardwareConfiguration Parse(string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
        {
            throw new ArgumentException("The XML content cannot be null or whitespace.", nameof(xmlContent));
        }

        Validate(xmlContent);

        var document = XDocument.Parse(xmlContent, LoadOptions.None);
        var devices = document
            .Root?
            .Elements("Device")
            .Select(ParseDevice)
            .ToArray() ?? Array.Empty<DeviceConfiguration>();

        if (devices.Length == 0)
        {
            throw new InvalidOperationException("The configuration does not contain any device definitions.");
        }

        _logger.LogInformation("Parsed {Count} device entries from configuration XML.", devices.Length);
        return new HardwareConfiguration(devices);
    }

    private void Validate(string xmlContent)
    {
        var errors = new List<string>();
        var settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
            Schemas = _schemaSet,
            DtdProcessing = DtdProcessing.Prohibit
        };

        settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
        settings.ValidationEventHandler += (_, args) =>
        {
            errors.Add(args.Message);
        };

        using var stringReader = new StringReader(xmlContent);
        using var reader = XmlReader.Create(stringReader, settings);
        while (reader.Read())
        {
        }

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors);
            _logger.LogWarning("Configuration XML failed validation: {Message}", errorMessage);
            throw new InvalidOperationException($"Configuration XML failed validation: {errorMessage}");
        }
    }

    private static DeviceConfiguration ParseDevice(XElement deviceElement)
    {
        var name = (string?)deviceElement.Attribute("name");
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Each device element must specify a non-empty name attribute.");
        }

        var source = (string?)deviceElement.Attribute("source");
        var frequency = (string?)deviceElement.Element("Frequency");
        var power = (string?)deviceElement.Element("Power");
        var mode = (string?)deviceElement.Element("Mode");

        var parameters = deviceElement
            .Elements("Parameter")
            .Select(x => new DeviceParameter(
                (string?)x.Attribute("name") ?? throw new InvalidOperationException("Parameter elements must define a name."),
                (string?)x.Attribute("value") ?? throw new InvalidOperationException("Parameter elements must define a value.")))
            .ToList();

        return new DeviceConfiguration(name, source, frequency, power, mode, parameters);
    }
}
