using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using ConfigSetup.Domain.Models;

namespace ConfigSetup.Web.ViewModels;

/// <summary>
/// View model describing a device entry and its editable settings.
/// </summary>
public sealed class DeviceEditorViewModel
{
    private readonly List<ParameterEditorViewModel> _parameters;

    public DeviceEditorViewModel(
        string name,
        string? defaultSource = null,
        string? defaultFrequency = null,
        string? defaultPower = null,
        string? defaultMode = null,
        IEnumerable<ParameterEditorViewModel>? parameters = null,
        InstrumentConnectionEditorViewModel? connection = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Device name cannot be null or whitespace.", nameof(name));
        }

        Name = name.Trim();
        DefaultSource = (defaultSource ?? string.Empty).Trim();
        DefaultFrequency = (defaultFrequency ?? string.Empty).Trim();
        DefaultPower = (defaultPower ?? string.Empty).Trim();
        DefaultMode = (defaultMode ?? string.Empty).Trim();

        Source = DefaultSource;
        Frequency = DefaultFrequency;
        Power = DefaultPower;
        Mode = DefaultMode;

        Connection = connection ?? new InstrumentConnectionEditorViewModel();

        _parameters = parameters?.Select(parameter =>
            {
                ArgumentNullException.ThrowIfNull(parameter);
                return parameter.Clone();
            })
            .ToList() ?? new List<ParameterEditorViewModel>();
    }

    public string Name { get; }

    public string DefaultSource { get; }

    public string DefaultFrequency { get; }

    public string DefaultPower { get; }

    public string DefaultMode { get; }

    public string Source { get; set; }

    public string Frequency { get; set; }

    public string Power { get; set; }

    public string Mode { get; set; }

    public InstrumentConnectionEditorViewModel Connection { get; }

    public IList<ParameterEditorViewModel> Parameters => _parameters;

    public void ResetToDefaults()
    {
        Source = DefaultSource;
        Frequency = DefaultFrequency;
        Power = DefaultPower;
        Mode = DefaultMode;
        Connection.ResetToDefault();

        foreach (var parameter in _parameters)
        {
            parameter.ResetToDefault();
        }
    }

    public void Apply(DeviceConfiguration? configuration)
    {
        if (configuration is null)
        {
            ResetToDefaults();
            return;
        }

        Source = string.IsNullOrWhiteSpace(configuration.Source) ? DefaultSource : configuration.Source.Trim();
        Frequency = string.IsNullOrWhiteSpace(configuration.Frequency) ? DefaultFrequency : configuration.Frequency.Trim();
        Power = string.IsNullOrWhiteSpace(configuration.Power) ? DefaultPower : configuration.Power.Trim();
        Mode = string.IsNullOrWhiteSpace(configuration.Mode) ? DefaultMode : configuration.Mode.Trim();
        Connection.Apply(configuration.Connection);

        foreach (var parameter in _parameters)
        {
            var matching = configuration.Parameters
                .FirstOrDefault(x => string.Equals(x.Name, parameter.Name, StringComparison.OrdinalIgnoreCase));
            parameter.Apply(matching?.Value);
        }

        foreach (var additional in configuration.Parameters)
        {
            if (_parameters.Any(parameter => string.Equals(parameter.Name, additional.Name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var parameterViewModel = new ParameterEditorViewModel(additional.Name, additional.Value);
            parameterViewModel.Apply(additional.Value);
            _parameters.Add(parameterViewModel);
        }
    }

    public bool Represents(DeviceConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return string.Equals(Name, configuration.Name, StringComparison.OrdinalIgnoreCase);
    }

    public XElement ToXElement()
    {
        var element = new XElement("Device", new XAttribute("name", Name));

        var sourceValue = string.IsNullOrWhiteSpace(Source) ? DefaultSource : Source.Trim();
        if (!string.IsNullOrWhiteSpace(sourceValue))
        {
            element.SetAttributeValue("source", sourceValue);
        }

        AppendElement(element, "Frequency", Frequency, DefaultFrequency);
        AppendElement(element, "Power", Power, DefaultPower);
        AppendElement(element, "Mode", Mode, DefaultMode);

        var connectionElement = Connection.ToXElement();
        if (connectionElement is not null)
        {
            element.Add(connectionElement);
        }

        foreach (var parameter in _parameters)
        {
            var value = parameter.ValueOrDefault;
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            element.Add(new XElement(
                "Parameter",
                new XAttribute("name", parameter.Name),
                new XAttribute("value", value)));
        }

        return element;
    }

    public static DeviceEditorViewModel FromConfiguration(DeviceConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var parameters = configuration.Parameters
            .Select(parameter => new ParameterEditorViewModel(parameter.Name, parameter.Value))
            .ToArray();

        var viewModel = new DeviceEditorViewModel(
            configuration.Name,
            configuration.Source,
            configuration.Frequency,
            configuration.Power,
            configuration.Mode,
            parameters,
            new InstrumentConnectionEditorViewModel(
                configuration.Connection?.Protocol,
                configuration.Connection?.Host,
                configuration.Connection?.Port));

        viewModel.Apply(configuration);
        return viewModel;
    }

    private static void AppendElement(XContainer container, string elementName, string value, string defaultValue)
    {
        var content = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        container.Add(new XElement(elementName, content));
    }
}
