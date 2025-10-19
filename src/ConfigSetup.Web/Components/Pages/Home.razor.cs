using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using ConfigSetup.Application.Configuration;
using ConfigSetup.Application.Scpi;
using ConfigSetup.Domain.Models;
using ConfigSetup.Domain.Scpi;
using ConfigSetup.Web.Contracts;
using ConfigSetup.Web.Services;
using ConfigSetup.Web.ViewModels;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace ConfigSetup.Web.Components.Pages;

public sealed partial class Home
{
    private const int MaxUploadSizeInBytes = 256 * 1024;

    private static readonly string SampleConfigurationXml = """
<Configuration>
  <Device name="Arb" source="Arb">
    <Frequency>1.2GHz</Frequency>
    <Power>-10dBm</Power>
    <Mode>FDM</Mode>
    <Parameter name="WAVE:TYPE" value="SINE" />
  </Device>
</Configuration>
""";

    [Inject]
    private IXmlConfigurationParser Parser { get; set; } = default!;

    [Inject]
    private IScpiCommandGenerator CommandGenerator { get; set; } = default!;

    [Inject]
    private ILogger<Home> Logger { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject]
    private ConfigurationExportService ExportService { get; set; } = default!;

    private string? XmlContent { get; set; }

    private bool IsProcessing { get; set; }

    private bool IsExporting { get; set; }

    private string? ErrorMessage { get; set; }

    private string? SuccessMessage { get; set; }

    private List<ScpiCommand> GeneratedCommands { get; } = new();

    private List<DeviceEditorViewModel> DeviceEditors { get; } = new();

    private DeviceEditorViewModel? SelectedDevice { get; set; }

    private InputFile? HiddenFileInput { get; set; }

    private MenuType? ActiveMenu { get; set; }

    private bool IsXmlPreviewVisible { get; set; }

    private string GenerateButtonLabel => IsProcessing ? "Processing..." : "Generate commands";

    protected override void OnInitialized()
    {
        InitializeDefaultEditors();
    }

    private async Task TriggerFilePickerAsync()
    {
        if (HiddenFileInput is null)
        {
            return;
        }

        await JsRuntime.InvokeVoidAsync("configSetup.triggerFilePicker", HiddenFileInput.Element);
        CloseMenus();
    }

    private async Task OnFileSelectedAsync(InputFileChangeEventArgs args)
    {
        if (args.FileCount == 0)
        {
            return;
        }

        var file = args.File;
        await using var stream = file.OpenReadStream(MaxUploadSizeInBytes);
        using var reader = new StreamReader(stream);
        XmlContent = await reader.ReadToEndAsync();

        GeneratedCommands.Clear();
        ClearAlerts();

        if (TryParseConfiguration(XmlContent, out var configuration))
        {
            ApplyConfigurationToEditors(configuration!);
            ShowSuccess($"Loaded configuration from '{file.Name}' and applied it to the UI.");
        }

        CloseMenus();
    }

    private async Task ExportXmlAsync()
    {
        try
        {
            IsExporting = true;
            ClearAlerts();

            var request = BuildExportRequest();
            var document = ExportService.CreateDocument(request);
            var xml = document.ToString();

            XmlContent = xml;
            await JsRuntime.InvokeVoidAsync("configSetup.downloadFile", $"configuration-{DateTime.UtcNow:yyyyMMddHHmmss}.xml", xml);
            ShowSuccess("Exported configuration as XML.");
        }
        catch (InvalidOperationException ex)
        {
            Logger.LogWarning(ex, "Validation failed while exporting configuration.");
            ShowError(ex.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to export configuration.");
            ShowError("An unexpected error occurred while exporting the configuration.");
        }
        finally
        {
            IsExporting = false;
            CloseMenus();
        }
    }

    private Task GenerateCommandsAsync()
    {
        if (!TryParseConfiguration(XmlContent, out var configuration))
        {
            GeneratedCommands.Clear();
            return Task.CompletedTask;
        }

        try
        {
            IsProcessing = true;
            ClearAlerts();

            ApplyConfigurationToEditors(configuration!);
            var commands = CommandGenerator.Generate(configuration!);

            GeneratedCommands.Clear();
            GeneratedCommands.AddRange(commands);

            ShowSuccess($"Generated {GeneratedCommands.Count} SCPI commands.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to generate commands from XML input.");
            ShowError(ex.Message);
            GeneratedCommands.Clear();
        }
        finally
        {
            IsProcessing = false;
        }

        return Task.CompletedTask;
    }

    private void OnLoadSampleClicked()
    {
        LoadSample();
        CloseMenus();
    }

    private void OnApplyXmlClicked()
    {
        SyncEditorsFromXml();
        CloseMenus();
    }

    private void OnUpdateXmlClicked()
    {
        SyncEditorsToXml();
        CloseMenus();
    }

    private void OnResetDeviceClicked()
    {
        ResetSelectedDevice();
        CloseMenus();
    }

    private void LoadSample()
    {
        XmlContent = SampleConfigurationXml;
        GeneratedCommands.Clear();
        ClearAlerts();

        if (TryParseConfiguration(XmlContent, out var configuration))
        {
            ApplyConfigurationToEditors(configuration!);
            ShowSuccess("Sample configuration loaded. Review and adjust as needed.");
        }
    }

    private void SyncEditorsFromXml()
    {
        GeneratedCommands.Clear();

        if (!TryParseConfiguration(XmlContent, out var configuration))
        {
            return;
        }

        ApplyConfigurationToEditors(configuration!);
        ShowSuccess($"Applied configuration for {configuration.Devices.Count} device(s) to the UI.");
    }

    private void SyncEditorsToXml()
    {
        try
        {
            XmlContent = BuildXmlFromEditors();
            GeneratedCommands.Clear();
            ShowSuccess("Updated XML with the current UI values.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to generate configuration XML from the UI state.");
            ShowError("Failed to build configuration XML from the current UI values.");
        }
    }

    private string BuildXmlFromEditors()
    {
        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("Configuration", DeviceEditors.Select(editor => editor.ToXElement())));

        return document.ToString();
    }

    private ExportConfigurationRequest BuildExportRequest()
    {
        var request = new ExportConfigurationRequest();

        foreach (var editor in DeviceEditors)
        {
            var device = new ExportDeviceRequest
            {
                Name = editor.Name,
                Source = CurrentOrDefault(editor.Source, editor.DefaultSource),
                Frequency = CurrentOrDefault(editor.Frequency, editor.DefaultFrequency),
                Power = CurrentOrDefault(editor.Power, editor.DefaultPower),
                Mode = CurrentOrDefault(editor.Mode, editor.DefaultMode)
            };

            foreach (var parameter in editor.Parameters)
            {
                device.Parameters.Add(new ExportParameterRequest
                {
                    Name = parameter.Name,
                    Value = parameter.ValueOrDefault
                });
            }

            request.Devices.Add(device);
        }

        return request;
    }

    private void ToggleXmlPreview()
    {
        IsXmlPreviewVisible = !IsXmlPreviewVisible;
        CloseMenus();
    }

    private void CloseMenus()
    {
        ActiveMenu = null;
    }

    private void ToggleMenu(MenuType menu)
    {
        ActiveMenu = ActiveMenu == menu ? null : menu;
    }

    private void ResetSelectedDevice()
    {
        if (SelectedDevice is null)
        {
            return;
        }

        SelectedDevice.ResetToDefaults();
        GeneratedCommands.Clear();
        ShowSuccess($"Reset '{SelectedDevice.Name}' to default values.");
    }

    private void ResetParameter(ParameterEditorViewModel parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);
        parameter.ResetToDefault();
    }

    private void SelectDevice(DeviceEditorViewModel device)
    {
        ArgumentNullException.ThrowIfNull(device);
        SelectedDevice = device;
    }

    private bool TryParseConfiguration(string? xmlContent, [NotNullWhen(true)] out HardwareConfiguration? configuration, bool reportIssues = true)
    {
        configuration = null;

        if (string.IsNullOrWhiteSpace(xmlContent))
        {
            if (reportIssues)
            {
                ShowError("Please provide configuration XML before performing this action.");
            }

            return false;
        }

        try
        {
            configuration = Parser.Parse(xmlContent);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to parse configuration XML.");

            if (reportIssues)
            {
                ShowError(ex.Message);
            }

            return false;
        }
    }

    private void ApplyConfigurationToEditors(HardwareConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        foreach (var editor in DeviceEditors)
        {
            var matching = configuration.Devices.FirstOrDefault(editor.Represents);
            editor.Apply(matching);
        }

        foreach (var device in configuration.Devices)
        {
            if (DeviceEditors.Any(editor => editor.Represents(device)))
            {
                continue;
            }

            DeviceEditors.Add(DeviceEditorViewModel.FromConfiguration(device));
        }

        SelectedDevice ??= DeviceEditors.FirstOrDefault();
    }

    private void InitializeDefaultEditors()
    {
        DeviceEditors.Clear();
        DeviceEditors.Add(new DeviceEditorViewModel(
            "Arb",
            defaultSource: "Arb",
            defaultFrequency: "1.2GHz",
            defaultPower: "-10dBm",
            defaultMode: "FDM",
            parameters: new[]
            {
                new ParameterEditorViewModel("WAVE:TYPE", "SINE"),
                new ParameterEditorViewModel("WAVE:FREQ", "1kHz"),
                new ParameterEditorViewModel("WAVE:AMPL", "1.0")
            }));

        SelectedDevice = DeviceEditors.FirstOrDefault();
    }

    private string GetDeviceButtonClass(DeviceEditorViewModel device)
    {
        ArgumentNullException.ThrowIfNull(device);
        var baseClass = "list-group-item list-group-item-action d-flex justify-content-between align-items-center slicer-sidebar-item";
        return ReferenceEquals(device, SelectedDevice) ? $"{baseClass} active" : baseClass;
    }

    private string GetMenuToggleClass(MenuType menu)
    {
        return ActiveMenu == menu ? "active" : string.Empty;
    }

    private static string FormatDefault(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "None" : value.Trim();
    }

    private static string? CurrentOrDefault(string? value, string defaultValue)
    {
        var content = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return string.IsNullOrWhiteSpace(content) ? null : content;
    }

    private void ClearAlerts()
    {
        ErrorMessage = null;
        SuccessMessage = null;
    }

    private void ShowError(string message)
    {
        ErrorMessage = message;
        SuccessMessage = null;
    }

    private void ShowSuccess(string message)
    {
        SuccessMessage = message;
        ErrorMessage = null;
    }

    private enum MenuType
    {
        File,
        Edit,
        View
    }
}
