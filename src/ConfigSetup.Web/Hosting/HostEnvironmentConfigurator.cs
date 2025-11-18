using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace ConfigSetup.Web.Hosting;

public static class HostEnvironmentConfigurator
{
    public static void Configure(WebApplicationBuilder builder, ServiceHostOptions options)
    {
        if (options.EnableWindowsServiceIntegration)
        {
            builder.Host.UseWindowsService();
        }

        if (options.EnableSystemdIntegration)
        {
            builder.Host.UseSystemd();
        }

        builder.WebHost.ConfigureKestrel((_, kestrelOptions) =>
        {
            kestrelOptions.AddServerHeader = false;

            if (options.HttpPort is int httpPort)
            {
                kestrelOptions.ListenAnyIP(httpPort, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                });
            }

            if (options.HttpsPort is int httpsPort)
            {
                kestrelOptions.ListenAnyIP(httpsPort, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    listenOptions.UseHttps();
                });
            }
        });
    }
}
