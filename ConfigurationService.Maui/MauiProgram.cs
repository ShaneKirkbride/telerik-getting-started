using ConfigurationService.Application;
using ConfigurationService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace ConfigurationService.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<ITaggedConfigParser, XmlTaggedConfigParser>();

        builder.Services.AddSingleton<IConfigPusher, GrpcAgentClient>();

        // Pushers: you can have multiple; Distributor will pick the first that returns true
        builder.Services.AddHttpClient<HttpAgentClient>();
        builder.Services.AddSingleton<IConfigPusher>(sp => sp.GetRequiredService<HttpAgentClient>());

        builder.Services.AddSingleton<IConfigDistributor, ConfigDistributor>();

        return builder.Build();
	}
}
