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

## Testing

```bash
dotnet test ConfigSetup.sln
```

The web application allows users to upload or paste XML and inspect the generated SCPI commands directly in the browser.
