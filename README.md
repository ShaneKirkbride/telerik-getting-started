# telerik-getting-started

This repository contains a basic Blazor Server application. The sample demonstrates
how to build a simple web interface using Razor components and dependency
injection.

## Building the project

The solution targets .NET 7.0. Restore and build using the .NET CLI:

```bash
dotnet build telerik-getting-started.sln
```

## Running the application

Launch the development server with:

```bash
dotnet run --project telerik-getting-started
```

The application registers `IWeatherForecastService` for retrieving weather data.
This interface allows alternative implementations to be injected for testing or
future enhancements.
