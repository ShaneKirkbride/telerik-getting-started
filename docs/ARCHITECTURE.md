# ConfigSetup architecture overview

## Purpose
ConfigSetup is a Blazor Server utility that ingests XML-based instrument definitions, validates them against an embedded schema, and turns the sanitized model into SCPI command sequences. The same host can simulate source uploads and drive real instruments through VISA-compatible sessions or OpenTAP sequences.

## Solution layout and responsibilities
- **ConfigSetup.Domain** – Domain models and validation assets. The `HardwareConfiguration` aggregate ensures at least one device is defined, while the schema provider exposes the embedded `Configuration.xsd` for XML validation.
- **ConfigSetup.Application** – Use cases and integrations. The XML parser validates incoming content against the schema and materializes strongly typed device configurations. SCPI generation builds deterministic command lists from the parsed model. Instrumentation abstractions (session factory, executor, VISA interop) and OpenTAP sequence parsing/execution live here to keep the web host thin.
- **ConfigSetup.Web** – Blazor Server host and UI. `Program.cs` wires the DI container, hosts Razor components, exposes a `/api/config/export` endpoint for exporting XML, and launches the browser when configured. The `ConfigurationExportService` assembles XML documents from user input before they are persisted or replayed elsewhere.
- **ConfigSetup.Tests** – xUnit coverage for configuration parsing, SCPI generation, and Blazor workspace state, preventing regressions in the core flows.

## End-to-end flow
1. Users supply configuration XML in the web UI or through the export endpoint.
2. `XmlConfigurationParser` validates the payload against the embedded schema and produces a `HardwareConfiguration` aggregate.
3. `ScpiCommandGenerator` transforms the aggregate into ordered SCPI commands, normalizing sources and parameters as needed.
4. Commands can be previewed in the UI, sent to an instrument via `InstrumentCommandExecutor`, or translated into OpenTAP sequences for automated execution.

## Hosting notes
The web host is preconfigured for HTTPS redirection, Windows service integration, and systemd integration. A publish profile ships a single-file Windows executable so the same binaries can run interactively on a desktop or unattended as a service.
