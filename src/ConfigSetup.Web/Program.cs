using ConfigSetup.Application.Configuration;
using ConfigSetup.Application.Scpi;
using ConfigSetup.Domain.Schemas;
using ConfigSetup.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton(ConfigurationSchemaProvider.CreateSchemaSet());
builder.Services.AddSingleton<IXmlConfigurationParser, XmlConfigurationParser>();
builder.Services.AddSingleton<IScpiCommandGenerator, ScpiCommandGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
