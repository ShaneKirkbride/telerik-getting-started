using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using ConfigSetup.Application.Configuration;
using ConfigSetup.Application.Instrumentation;
using ConfigSetup.Application.OpenTap;
using ConfigSetup.Application.Scpi;
using ConfigSetup.Domain.Models;
using ConfigSetup.Domain.Scpi;
using ConfigSetup.Web.Contracts;
using ConfigSetup.Web.Services;
using ConfigSetup.Web.ViewModels;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace ConfigSetup.Web.Components.Pages;

public sealed partial class Home
{
    private const int MaxUploadSizeInBytes = 256 * 1024;
    private const int MaxSequenceUploadSizeInBytes = 256 * 1024;

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
    private IInstrumentCommandExecutor InstrumentCommandExecutor { get; set; } = default!;

    [Inject]
    private IOpenTapSequenceExecutor OpenTapSequenceExecutor { get; set; } = default!;

    [Inject]
    private ILogger<Home> Logger { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject]
    private ConfigurationExportService ExportService { get; set; } = default!;

    [Inject]
    private ISourceUploadService SourceUploadService { get; set; } = default!;

    [Inject]
    private IOptions<InstrumentOptions> InstrumentOptions { get; set; } = default!;

    private string? XmlContent { get; set; }

    private bool IsProcessing { get; set; }

    private bool IsExporting { get; set; }

    private bool IsUploading { get; set; }

    private bool IsInstrumentBusy { get; set; }

    private string? ErrorMessage { get; set; }

    private string? SuccessMessage { get; set; }

    private List<ScpiCommand> GeneratedCommands { get; } = new();

    private List<DeviceEditorViewModel> DeviceEditors { get; } = new();

    private DeviceEditorViewModel? SelectedDevice { get; set; }

    private InputFile? HiddenFileInput { get; set; }

    private InputFile? OpenTapFileInput { get; set; }

    private MenuType? ActiveMenu { get; set; }

    private bool IsXmlPreviewVisible { get; set; }

    private string InstrumentResourceAddress { get; set; } = string.Empty;

    private string? CustomCommandText { get; set; }

    private InstrumentExecutionResult? LastInstrumentExecution { get; set; }

    private string GenerateButtonLabel => IsProcessing ? "Processing..." : "Generate commands";

    private ScpiWorkspaceState ScpiWorkspace { get; } = new();

    private EditorWorkspaceState EditorWorkspace { get; } = new();

    protected override void OnInitialized()
    {
        InitializeDefaultEditors();
        InstrumentResourceAddress = InstrumentOptions.Value.DefaultResourceAddress ?? string.Empty;
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

    private Task UploadAllSourcesAsync()
    {
        return UploadSourcesAsync(DeviceEditors);
    }

    private Task UploadSourceAsync(DeviceEditorViewModel device)
    {
        ArgumentNullException.ThrowIfNull(device);
        return UploadSourcesAsync(new[] { device });
    }

    private Task ExecuteGeneratedCommandsOnInstrumentAsync()
    {
        if (GeneratedCommands.Count == 0)
        {
            ShowError("Generate SCPI commands before streaming them to an instrument.");
            return Task.CompletedTask;
        }

        return ExecuteCommandsWithInstrumentAsync(GeneratedCommands, "Generated command list");
    }

    private Task SendCustomCommandsAsync()
    {
        var commands = (CustomCommandText ?? string.Empty)
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => new ScpiCommand(line.Trim()))
            .Where(command => !string.IsNullOrWhiteSpace(command.Text))
            .ToList();

        if (commands.Count == 0)
        {
            ShowError("Provide at least one SCPI command before sending it to an instrument.");
            return Task.CompletedTask;
        }

        return ExecuteCommandsWithInstrumentAsync(commands, "Custom instrument commands");
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

    private async Task TriggerSequencePickerAsync()
    {
        if (OpenTapFileInput is null)
        {
            return;
        }

        await JsRuntime.InvokeVoidAsync("configSetup.triggerFilePicker", OpenTapFileInput.Element);
    }

    private async Task OnOpenTapSequenceSelectedAsync(InputFileChangeEventArgs args)
    {
        if (args.FileCount == 0)
        {
            return;
        }

        var file = args.File;
        await using var stream = file.OpenReadStream(MaxSequenceUploadSizeInBytes);
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        await ExecuteOpenTapSequenceAsync(content, file.Name);
    }

    private void ResetSelectedDevice()
    {
        if (SelectedDevice is null)
        {
            return;
        }

        ResetDevice(SelectedDevice);
    }

    private void ResetDevice(DeviceEditorViewModel device)
    {
        ArgumentNullException.ThrowIfNull(device);
        device.ResetToDefaults();
        GeneratedCommands.Clear();
        ShowSuccess($"Reset '{device.Name}' to default values.");
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
        EditorWorkspace.SetActiveTab(EditorWorkspaceTab.SourceEditor);
    }

    private bool IsDeviceSelected(DeviceEditorViewModel device)
    {
        ArgumentNullException.ThrowIfNull(device);
        return ReferenceEquals(device, SelectedDevice);
    }

    private string GetSourceListItemClass(DeviceEditorViewModel device)
    {
        ArgumentNullException.ThrowIfNull(device);
        return IsDeviceSelected(device) ? "source-list-item active" : "source-list-item";
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
        DeviceEditors.AddRange(DeviceEditorTemplateCatalog.CreateDefaultEditors());

        SelectedDevice = DeviceEditors.FirstOrDefault();
    }

    private string GetMenuToggleClass(MenuType menu)
    {
        return ActiveMenu == menu ? "active" : string.Empty;
    }

    private void SetScpiTab(ScpiWorkspaceTab tab)
    {
        ScpiWorkspace.SetActiveTab(tab);
    }

    private string GetScpiTabClass(ScpiWorkspaceTab tab)
    {
        return ScpiWorkspace.GetTabCss(tab);
    }

    private string GetScpiPaneClass(ScpiWorkspaceTab tab)
    {
        return ScpiWorkspace.GetPaneCss(tab);
    }

    private void SetEditorWorkspaceTab(EditorWorkspaceTab tab)
    {
        EditorWorkspace.SetActiveTab(tab);
    }

    private string GetEditorWorkspaceTabClass(EditorWorkspaceTab tab)
    {
        return EditorWorkspace.GetTabCss(tab);
    }

    private string GetEditorWorkspacePaneClass(EditorWorkspaceTab tab)
    {
        return EditorWorkspace.GetPaneCss(tab);
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

    private async Task UploadSourcesAsync(IEnumerable<DeviceEditorViewModel> devices)
    {
        ArgumentNullException.ThrowIfNull(devices);

        var deviceList = devices.Where(static editor => editor is not null).ToList();
        if (deviceList.Count == 0)
        {
            ShowError("No sources are available to upload.");
            return;
        }

        try
        {
            IsUploading = true;
            ClearAlerts();

            var requests = deviceList
                .Select(BuildUploadRequest)
                .ToArray();

            var result = await SourceUploadService.UploadAsync(requests);
            ShowSuccess($"Uploaded {result.UploadedSources} source(s) in {result.Duration.TotalMilliseconds:F0} ms. {result.TotalParameters} parameter value(s) applied.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to upload source settings.");
            ShowError("An error occurred while uploading the source settings.");
        }
        finally
        {
            IsUploading = false;
        }
    }

    private static SourceUploadRequest BuildUploadRequest(DeviceEditorViewModel editor)
    {
        ArgumentNullException.ThrowIfNull(editor);

        return new SourceUploadRequest
        {
            Name = editor.Name,
            Source = CurrentOrDefault(editor.Source, editor.DefaultSource),
            Frequency = CurrentOrDefault(editor.Frequency, editor.DefaultFrequency),
            Power = CurrentOrDefault(editor.Power, editor.DefaultPower),
            Mode = CurrentOrDefault(editor.Mode, editor.DefaultMode),
            Parameters = editor.Parameters
                .Where(parameter => !string.IsNullOrWhiteSpace(parameter.ValueOrDefault))
                .Select(parameter => new SourceUploadParameter(parameter.Name, parameter.ValueOrDefault))
                .ToList()
        };
    }

    private async Task ExecuteOpenTapSequenceAsync(string content, string? fileName)
    {
        if (!TryGetInstrumentAddress(out var resourceAddress))
        {
            return;
        }

        try
        {
            IsInstrumentBusy = true;
            ClearAlerts();

            var result = await OpenTapSequenceExecutor.ExecuteAsync(content, resourceAddress);
            LastInstrumentExecution = result;
            var sequenceName = result.SequenceName ?? fileName ?? "OpenTAP sequence";
            ShowSuccess($"Executed '{sequenceName}' with {result.CommandCount} SCPI command(s).");
        }
        catch (Exception ex)
        {
            LastInstrumentExecution = null;
            Logger.LogError(ex, "Failed to execute OpenTAP sequence.");
            ShowError("Unable to execute the OpenTAP sequence. Verify the file format and instrument connection.");
        }
        finally
        {
            IsInstrumentBusy = false;
        }
    }

    private async Task ExecuteCommandsWithInstrumentAsync(IEnumerable<ScpiCommand> commands, string? sequenceName)
    {
        ArgumentNullException.ThrowIfNull(commands);

        if (!TryGetInstrumentAddress(out var resourceAddress))
        {
            return;
        }

        var commandList = commands.Where(static command => !string.IsNullOrWhiteSpace(command.Text)).ToList();
        if (commandList.Count == 0)
        {
            ShowError("No SCPI commands are available to execute.");
            return;
        }

        try
        {
            IsInstrumentBusy = true;
            ClearAlerts();

            var context = new InstrumentExecutionContext(resourceAddress, sequenceName);
            LastInstrumentExecution = await InstrumentCommandExecutor.ExecuteAsync(context, commandList);
            ShowSuccess($"Sent {LastInstrumentExecution.CommandCount} SCPI command(s) to {LastInstrumentExecution.ResourceAddress}.");
        }
        catch (Exception ex)
        {
            LastInstrumentExecution = null;
            Logger.LogError(ex, "Failed to stream SCPI commands to the instrument.");
            ShowError("Unable to send commands to the instrument. Check the VISA resource and cabling.");
        }
        finally
        {
            IsInstrumentBusy = false;
        }
    }

    private bool TryGetInstrumentAddress(out string resourceAddress)
    {
        resourceAddress = InstrumentResourceAddress?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(resourceAddress))
        {
            ShowError("Specify a VISA resource address before executing instrument commands.");
            return false;
        }

        return true;
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
