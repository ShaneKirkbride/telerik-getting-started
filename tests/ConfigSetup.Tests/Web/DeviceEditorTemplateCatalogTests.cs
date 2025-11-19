using System;
using System.Linq;

using ConfigSetup.Web.ViewModels;

namespace ConfigSetup.Tests.Web;

public sealed class DeviceEditorTemplateCatalogTests
{
    [Fact]
    public void CreateDefaultEditors_SeedsFourDistinctDevices()
    {
        var editors = DeviceEditorTemplateCatalog.CreateDefaultEditors();

        Assert.Equal(4, editors.Count);
        Assert.Equal(4, editors.Select(editor => editor.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void CreateDefaultEditors_IncludesRichDefaultsForFirstEntry()
    {
        var editors = DeviceEditorTemplateCatalog.CreateDefaultEditors();
        var pulseEditor = editors.First();

        Assert.Equal("Pulse Arb", pulseEditor.Name);
        Assert.Equal("ARB-A", pulseEditor.DefaultSource);
        Assert.Equal("1.200GHz", pulseEditor.DefaultFrequency);
        Assert.Equal("-10dBm", pulseEditor.DefaultPower);
        Assert.Equal("FDM", pulseEditor.DefaultMode);
        Assert.Equal(3, pulseEditor.Parameters.Count);
    }

    [Fact]
    public void CreateDefaultEditors_AssignsDistinctParameterSetsPerDevice()
    {
        var editors = DeviceEditorTemplateCatalog.CreateDefaultEditors();

        var signatures = editors
            .Select(editor => string.Join(';', editor.Parameters.Select(parameter => $"{parameter.Name}={parameter.DefaultValue}")))
            .ToList();

        Assert.Equal(editors.Count, signatures.Distinct(StringComparer.Ordinal).Count());
    }
}
