# ConfigSetup

ConfigSetup is a lightweight **Blazor-only** (no MAUI dependencies) server application that ingests instrument configuration XML, validates it against an embedded schema, and produces SCPI command sequences for review.

## Solution layout

```
ConfigSetup.sln
├── src
│   ├── ConfigSetup.Domain          # Domain models, schema provider, SCPI primitives
│   ├── ConfigSetup.Application     # XML parsing, validation, and SCPI generation services
│   └── ConfigSetup.Web             # Blazor Server UI host
└── tests
    └── ConfigSetup.Tests           # xUnit tests for parsing and SCPI logic
```

## Prerequisites

* .NET SDK 9.0 or later

## Building and running

```bash
dotnet build ConfigSetup.sln
dotnet run --project src/ConfigSetup.Web/ConfigSetup.Web.csproj
```

## Packaging a versioned Windows executable

1. Update the `<Version>` property inside `src/ConfigSetup.Web/ConfigSetup.Web.csproj` to the number you want baked into the desktop binary title. The project is configured to emit an assembly named `ConfigSetup.Web-v<Version>`, so publishing automatically yields an `.exe` such as `ConfigSetup.Web-v1.0.0.exe`.
2. Publish the Blazor Server host as a single-file, self-contained executable. A publish profile named `SingleFileWin64` is checked in so you can run one command without memorizing all of the switches:

   ```bash
   dotnet publish src/ConfigSetup.Web/ConfigSetup.Web.csproj -p:PublishProfile=SingleFileWin64
   ```

   The profile pins the build to `Release`, targets the `net8.0` TFMs for maximum runtime compatibility, and emits a compressed `win-x64` executable that self-extracts any native dependencies at startup.

3. Copy the generated file from `src/ConfigSetup.Web/bin/Release/net8.0/win-x64/publish/` to your desktop and launch it directly. The executable self-hosts Kestrel, so no MAUI WebView or native shell is required.

## Hosting as a Windows service or Linux systemd unit

The `ConfigSetup.Web` host wires into the ASP.NET Core Windows Service and systemd integrations so the exact same binaries can run unattended inside a larger framework. The behavior is controlled through the `ServiceHost` section in `appsettings.json`:

```json
"ServiceHost": {
  "EnableWindowsServiceIntegration": true,
  "EnableSystemdIntegration": true,
  "EnableHttpsRedirection": true,
  "AutoLaunchBrowser": true,
  "HttpPort": 5125,
  "HttpsPort": 7125
}
```

* Toggle the integration flags off when you want to run the executable strictly as a desktop app.
* When running behind an upstream reverse proxy, disable `EnableHttpsRedirection` to prevent redirect loops.
* Leave `AutoLaunchBrowser` enabled so the executable automatically opens your default browser when it runs interactively. Turn it off if the app should only listen in the background.
* Provide `HttpPort` and/or `HttpsPort` values to pin the ports when running as a background service; otherwise Kestrel falls back to ASP.NET Core defaults.

## Testing

```bash
dotnet test ConfigSetup.sln
```

The web application allows users to upload or paste XML and inspect the generated SCPI commands directly in the browser.
