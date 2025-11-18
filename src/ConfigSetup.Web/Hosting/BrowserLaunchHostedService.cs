using System.Diagnostics;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Systemd;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConfigSetup.Web.Hosting;

public sealed class BrowserLaunchHostedService : IHostedService
{
    private static readonly TimeSpan AddressDiscoveryTimeout = TimeSpan.FromSeconds(10);

    private readonly IServer _server;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<BrowserLaunchHostedService> _logger;
    private readonly ServiceHostOptions _options;

    public BrowserLaunchHostedService(
        IServer server,
        IHostApplicationLifetime applicationLifetime,
        IOptions<ServiceHostOptions> options,
        ILogger<BrowserLaunchHostedService> logger)
    {
        _server = server;
        _applicationLifetime = applicationLifetime;
        _logger = logger;
        _options = options.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!ShouldLaunchBrowser())
        {
            return Task.CompletedTask;
        }

        _applicationLifetime.ApplicationStarted.Register(() => _ = Task.Run(LaunchBrowserAsync, CancellationToken.None));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private bool ShouldLaunchBrowser()
    {
        if (!_options.AutoLaunchBrowser)
        {
            return false;
        }

        if (WindowsServiceHelpers.IsWindowsService())
        {
            return false;
        }

        if (SystemdHelpers.IsSystemdService())
        {
            return false;
        }

        if (OperatingSystem.IsLinux() && Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
        {
            return false;
        }

        return true;
    }

    private async Task LaunchBrowserAsync()
    {
        try
        {
            var feature = _server.Features.Get<IServerAddressesFeature>();
            if (feature is null)
            {
                _logger.LogWarning("Unable to detect server addresses. Skipping browser launch.");
                return;
            }

            var address = await WaitForAddressAsync(feature, _applicationLifetime.ApplicationStopping);
            if (address is null)
            {
                _logger.LogWarning("Timed out waiting for Kestrel to expose a listening address.");
                return;
            }

            var normalized = NormalizeAddress(address);
            _logger.LogInformation("Launching default browser at {Address}", normalized);

            Process.Start(new ProcessStartInfo
            {
                FileName = normalized,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to open the default browser automatically.");
        }
    }

    private static async Task<string?> WaitForAddressAsync(IServerAddressesFeature feature, CancellationToken cancellationToken)
    {
        var start = DateTime.UtcNow;
        while (!cancellationToken.IsCancellationRequested && DateTime.UtcNow - start < AddressDiscoveryTimeout)
        {
            var address = feature.Addresses.FirstOrDefault();
            if (address is not null)
            {
                return address;
            }

            try
            {
                await Task.Delay(200, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        return null;
    }

    private static string NormalizeAddress(string address)
    {
        if (!Uri.TryCreate(address, UriKind.Absolute, out var uri))
        {
            return address;
        }

        if (IPAddress.TryParse(uri.Host, out var ipAddress))
        {
            if (IPAddress.Any.Equals(ipAddress) || IPAddress.IPv6Any.Equals(ipAddress))
            {
                var builder = new UriBuilder(uri) { Host = "localhost" };
                return builder.Uri.ToString();
            }
        }

        return uri.ToString();
    }
}
