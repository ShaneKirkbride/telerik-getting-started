using System.Collections.Generic;

namespace ConfigSetup.Web.ViewModels;

/// <summary>
/// Provides seed device editor entries so the slicer UI starts with multiple sources.
/// </summary>
public static class DeviceEditorTemplateCatalog
{
    /// <summary>
    /// Creates the default editor entries shown when the UI first loads.
    /// </summary>
    public static IReadOnlyList<DeviceEditorViewModel> CreateDefaultEditors()
    {
        return new[]
        {
            CreatePulseArbTemplate(),
            CreateNoiseTemplate(),
            CreateVectorTemplate(),
            CreateCalibrationTemplate()
        };
    }

    private static DeviceEditorViewModel CreatePulseArbTemplate()
    {
        return new DeviceEditorViewModel(
            "Pulse Arb",
            defaultSource: "ARB-A",
            defaultFrequency: "1.200GHz",
            defaultPower: "-10dBm",
            defaultMode: "FDM",
            connection: new InstrumentConnectionEditorViewModel("HiSLIP", "192.168.0.20", "4880"),
            parameters: new[]
            {
                new ParameterEditorViewModel("WAVE:TYPE", "SINE"),
                new ParameterEditorViewModel("WAVE:FREQ", "1kHz"),
                new ParameterEditorViewModel("WAVE:AMPL", "1.0")
            });
    }

    private static DeviceEditorViewModel CreateNoiseTemplate()
    {
        return new DeviceEditorViewModel(
            "Noise Source",
            defaultSource: "NOISE-A",
            defaultFrequency: "500MHz",
            defaultPower: "-30dBm",
            defaultMode: "NOISE",
            connection: new InstrumentConnectionEditorViewModel("HiSLIP", "192.168.0.30", "4881"),
            parameters: new[]
            {
                new ParameterEditorViewModel("NOISE:BAND", "20MHz"),
                new ParameterEditorViewModel("NOISE:SHAP", "FLAT")
            });
    }

    private static DeviceEditorViewModel CreateVectorTemplate()
    {
        return new DeviceEditorViewModel(
            "Vector Source",
            defaultSource: "VEC-A",
            defaultFrequency: "2.400GHz",
            defaultPower: "0dBm",
            defaultMode: "IQ",
            connection: new InstrumentConnectionEditorViewModel("HiSLIP", "192.168.0.40", "4880"),
            parameters: new[]
            {
                new ParameterEditorViewModel("ARB:FILE", "LTE-REF"),
                new ParameterEditorViewModel("ARB:SRAT", "61.44Msps")
            });
    }

    private static DeviceEditorViewModel CreateCalibrationTemplate()
    {
        return new DeviceEditorViewModel(
            "Calibration Reference",
            defaultSource: "CAL-REF",
            defaultFrequency: "1GHz",
            defaultPower: "-3dBm",
            defaultMode: "CW",
            connection: new InstrumentConnectionEditorViewModel("TCPIP-SOCKET", "192.168.0.50", "5025"),
            parameters: new[]
            {
                new ParameterEditorViewModel("OUTP:STAT", "ON")
            });
    }
}
