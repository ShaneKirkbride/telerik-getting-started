namespace ConfigSetup.Domain.Models;

/// <summary>
/// Represents a named parameter captured from the configuration document.
/// </summary>
public sealed record DeviceParameter(string Name, string Value);
