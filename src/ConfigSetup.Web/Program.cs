using ConfigSetup.Application.Configuration;
using ConfigSetup.Application.Instrumentation;
using ConfigSetup.Application.OpenTap;
using ConfigSetup.Application.Scpi;
using ConfigSetup.Domain.Schemas;
using ConfigSetup.Web.Components;
using ConfigSetup.Web.Contracts;
using ConfigSetup.Web.Hosting;
using ConfigSetup.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var serviceHostOptions = builder.Configuration
    .GetSection(ServiceHostOptions.SectionName)
    .Get<ServiceHostOptions>() ?? new ServiceHostOptions();

HostEnvironmentConfigurator.Configure(builder, serviceHostOptions);
builder.Services.Configure<ServiceHostOptions>(builder.Configuration.GetSection(ServiceHostOptions.SectionName));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton(ConfigurationSchemaProvider.CreateSchemaSet());
builder.Services.AddSingleton<IXmlConfigurationParser, XmlConfigurationParser>();
builder.Services.AddSingleton<IScpiCommandGenerator, ScpiCommandGenerator>();
builder.Services.AddSingleton<ConfigurationExportService>();
builder.Services.AddSingleton<ISourceUploadService, SimulatedSourceUploadService>();
builder.Services.AddSingleton<IInstrumentSessionFactory, KeysightInstrumentSessionFactory>();
builder.Services.AddSingleton<IInstrumentCommandExecutor, InstrumentCommandExecutor>();
builder.Services.AddSingleton<IOpenTapSequenceParser, OpenTapSequenceParser>();
builder.Services.AddSingleton<IOpenTapSequenceExecutor, OpenTapSequenceExecutor>();
builder.Services.AddHostedService<BrowserLaunchHostedService>();
builder.Services.Configure<InstrumentOptions>(builder.Configuration.GetSection(InstrumentOptions.SectionName));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

if (serviceHostOptions.EnableHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/api/config/export", (ExportConfigurationRequest? request, ConfigurationExportService exportService) =>
{
    if (request is null)
    {
        return Results.BadRequest("A request body is required.");
    }

    try
    {
        var document = exportService.CreateDocument(request);
        return Results.Text(document.ToString(), "application/xml");
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (ArgumentNullException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.Run();
