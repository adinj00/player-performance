using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace PlayerPerformance.Api.Configuration;

internal static class ProductionServiceCollectionExtensions
{
    internal const string AuthSensitivePolicy = "AUTH_SENSITIVE";
    internal const string TokenSetupPolicy = "TOKEN_SETUP";
    internal const string UploadCreatePolicy = "UPLOAD_CREATE";

    public static IServiceCollection AddProductionConfiguration(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddSingleton<IValidateOptions<DeploymentOptions>, ProductionOptionsValidator>();
        services.AddSingleton<IValidateOptions<DataProtectionSettings>, DataProtectionSettingsValidator>();
        services.AddSingleton<IValidateOptions<RateLimitSettings>, RateLimitSettingsValidator>();
        services.AddOptions<DeploymentOptions>().Bind(configuration.GetSection(DeploymentOptions.SectionName)).ValidateOnStart();
        services.AddOptions<DataProtectionSettings>().Bind(configuration.GetSection(DataProtectionSettings.SectionName)).ValidateOnStart();
        services.AddOptions<RateLimitSettings>().Bind(configuration.GetSection(RateLimitSettings.SectionName)).ValidateOnStart();

        var settings = configuration.GetSection(RateLimitSettings.SectionName).Get<RateLimitSettings>() ?? new RateLimitSettings();
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = static (context, _) => { context.HttpContext.Response.Headers["Retry-After"] = "60"; return ValueTask.CompletedTask; };
            options.AddPolicy(AuthSensitivePolicy, context => FixedWindow(ClientPartition(context), settings.AuthSensitivePermitLimit, settings.WindowSeconds));
            options.AddPolicy(TokenSetupPolicy, context => FixedWindow(ClientPartition(context), settings.TokenSetupPermitLimit, settings.WindowSeconds));
            options.AddPolicy(UploadCreatePolicy, context => RateLimitPartition.GetConcurrencyLimiter(AuthenticatedUserOrClientPartition(context), _ => new ConcurrencyLimiterOptions
            {
                PermitLimit = settings.UploadCreatePermitLimit,
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
        });

        var protection = configuration.GetSection(DataProtectionSettings.SectionName).Get<DataProtectionSettings>();
        if (!string.IsNullOrWhiteSpace(protection?.ApplicationName) && !string.IsNullOrWhiteSpace(protection.KeysPath))
        {
            services.AddDataProtection()
                .SetApplicationName(protection.ApplicationName)
                .PersistKeysToFileSystem(new DirectoryInfo(protection.KeysPath));
        }
        else
            services.AddDataProtection();

        return services;
    }

    public static ForwardedHeadersOptions CreateForwardedHeadersOptions(DeploymentOptions options)
    {
        var result = new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto, ForwardLimit = 1 };
        if (!Enum.TryParse<TrustedProxyMode>(options.TrustedProxyMode, true, out var mode) || mode == TrustedProxyMode.DIRECT)
            return result;
        result.KnownProxies.Clear();
        result.KnownNetworks.Clear();
        if (mode == TrustedProxyMode.KNOWN_PROXIES)
            foreach (var proxy in options.KnownProxies)
                result.KnownProxies.Add(IPAddress.Parse(proxy));
        if (mode == TrustedProxyMode.KNOWN_NETWORKS)
            foreach (var network in options.KnownNetworks)
            {
                var pieces = network.Split('/', 2);
                result.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse(pieces[0]), int.Parse(pieces[1], System.Globalization.CultureInfo.InvariantCulture)));
            }
        return result;
    }

    private static RateLimitPartition<string> FixedWindow(string partition, int permits, int seconds) => RateLimitPartition.GetFixedWindowLimiter(partition, _ => new FixedWindowRateLimiterOptions
    {
        PermitLimit = permits,
        Window = TimeSpan.FromSeconds(seconds),
        QueueLimit = 0,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
    });
    private static string ClientPartition(HttpContext context) => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    private static string AuthenticatedUserOrClientPartition(HttpContext context) => context.User.Identity?.IsAuthenticated == true ? $"user:{context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown"}" : $"ip:{ClientPartition(context)}";
}
