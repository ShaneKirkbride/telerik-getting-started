using System;

namespace ConfigSetup.Web.ViewModels;

/// <summary>
/// Represents a single configurable parameter entry in the slicer-style UI.
/// </summary>
public sealed class ParameterEditorViewModel
{
    public ParameterEditorViewModel(string name, string? defaultValue = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Parameter name cannot be null or whitespace.", nameof(name));
        }

        Name = name.Trim();
        DefaultValue = defaultValue?.Trim() ?? string.Empty;
        Value = DefaultValue;
    }

    public string Name { get; }

    public string DefaultValue { get; }

    public string Value { get; set; }

    public string ValueOrDefault => string.IsNullOrWhiteSpace(Value) ? DefaultValue : Value.Trim();

    public void ResetToDefault()
    {
        Value = DefaultValue;
    }

    public void Apply(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? DefaultValue : value.Trim();
    }

    public ParameterEditorViewModel Clone()
    {
        return new ParameterEditorViewModel(Name, DefaultValue)
        {
            Value = Value
        };
    }
}
