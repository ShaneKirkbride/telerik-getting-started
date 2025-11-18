# ConfigSetup

ConfigSetup is a lightweight Blazor Server application that ingests instrument configuration XML, validates it against an embedded schema, and produces SCPI command sequences for review.

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
2. Publish the Blazor Server host as a single-file, self-contained executable for your chosen runtime identifier (replace `win-x64` if necessary):

   ```bash
   dotnet publish src/ConfigSetup.Web/ConfigSetup.Web.csproj \
     -c Release \
     -r win-x64 \
     --self-contained true \
     /p:PublishSingleFile=true \
     /p:IncludeNativeLibrariesForSelfExtract=true \
     /p:PublishTrimmed=false
   ```

3. Copy the generated file from `src/ConfigSetup.Web/bin/Release/net8.0/win-x64/publish/` to your desktop and launch it directly.

## Testing

```bash
dotnet test ConfigSetup.sln
```

The web application allows users to upload or paste XML and inspect the generated SCPI commands directly in the browser.
