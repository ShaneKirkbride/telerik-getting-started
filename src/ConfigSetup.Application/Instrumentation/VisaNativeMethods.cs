using System;
using System.Runtime.InteropServices;

namespace ConfigSetup.Application.Instrumentation;

internal static class VisaNativeMethods
{
    private const string VisaLibraryName = "visa32";
    private const uint VI_NO_LOCK = 0;
    private const uint VI_ATTR_TMO_VALUE = 0x3FFF001A;
    private const uint VI_ATTR_TERMCHAR_EN = 0x3FFF0038;
    private const uint VI_ATTR_TERMCHAR = 0x3FFF0018;
    private const int VI_SUCCESS = 0;

    [DllImport(VisaLibraryName, EntryPoint = "viOpenDefaultRM")]
    private static extern int viOpenDefaultRM(out int resourceManagerHandle);

    [DllImport(VisaLibraryName, EntryPoint = "viOpen")]
    private static extern int viOpen(int resourceManagerHandle, string resourceName, uint accessMode, uint timeout, out int sessionHandle);

    [DllImport(VisaLibraryName, EntryPoint = "viSetAttribute")]
    private static extern int viSetAttribute(int sessionHandle, uint attributeName, uint attributeValue);

    [DllImport(VisaLibraryName, EntryPoint = "viWrite")]
    private static extern int viWrite(int sessionHandle, byte[] buffer, uint count, out uint actualCount);

    [DllImport(VisaLibraryName, EntryPoint = "viClose")]
    private static extern int viClose(int sessionHandle);

    internal static int OpenDefaultResourceManager()
    {
        var status = viOpenDefaultRM(out var handle);
        EnsureSuccess(status, "Failed to open the Keysight VISA resource manager.");
        return handle;
    }

    internal static int OpenInstrument(int resourceManagerHandle, string resourceAddress)
    {
        var status = viOpen(resourceManagerHandle, resourceAddress, VI_NO_LOCK, 0, out var sessionHandle);
        EnsureSuccess(status, $"Failed to open VISA resource '{resourceAddress}'.");
        return sessionHandle;
    }

    internal static void ConfigureTimeout(int sessionHandle, uint timeoutMilliseconds)
    {
        var timeout = timeoutMilliseconds == 0 ? 1u : timeoutMilliseconds;
        var status = viSetAttribute(sessionHandle, VI_ATTR_TMO_VALUE, timeout);
        EnsureSuccess(status, "Failed to configure VISA timeout.");
    }

    internal static void ConfigureTermination(int sessionHandle, bool enable, byte terminationCharacter)
    {
        var status = viSetAttribute(sessionHandle, VI_ATTR_TERMCHAR_EN, enable ? 1u : 0u);
        EnsureSuccess(status, "Failed to configure VISA termination handling.");
        var charStatus = viSetAttribute(sessionHandle, VI_ATTR_TERMCHAR, terminationCharacter);
        EnsureSuccess(charStatus, "Failed to configure the VISA termination character.");
    }

    internal static void Write(int sessionHandle, byte[] buffer)
    {
        var status = viWrite(sessionHandle, buffer, (uint)buffer.Length, out _);
        EnsureSuccess(status, "Failed to write the SCPI command to the VISA session.");
    }

    internal static void Close(int sessionHandle)
    {
        if (sessionHandle == 0)
        {
            return;
        }

        viClose(sessionHandle);
    }

    private static void EnsureSuccess(int status, string message)
    {
        if (status < VI_SUCCESS)
        {
            throw new InvalidOperationException($"{message} (VISA status {status})");
        }
    }
}
