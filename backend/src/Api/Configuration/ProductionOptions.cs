using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using PlayerPerformance.Infrastructure.Files;
using PlayerPerformance.Infrastructure.Imports;

namespace PlayerPerformance.Api.Configuration;

internal enum DeploymentTopology
{
    SAME_ORIGIN,
    SPLIT_ORIGIN
}
internal enum TrustedProxyMode
{
    DIRECT,
    KNOWN_PROXIES,
    KNOWN_NETWORKS
}

internal sealed class DeploymentOptions
{
    internal const string SectionName = "Deployment";
    public string? Topology { get; init; }
    public string? PublicWebOrigin { get; init; }
    public string? PublicApiOrigin { get; init; }
    public string[] AllowedFrontendOrigins { get; init; } = [];
    public string? TrustedProxyMode { get; init; }
    public string[] KnownProxies { get; init; } = [];
    public string[] KnownNetworks { get; init; } = [];
    public string? BuildVersion { get; init; }
    public string? CommitSha { get; init; }

    internal bool IsSplitOrigin => string.Equals(Topology, nameof(DeploymentTopology.SPLIT_ORIGIN), StringComparison.OrdinalIgnoreCase);
}

internal sealed class DataProtectionSettings
{
    internal const string SectionName = "DataProtection";
    public string? ApplicationName { get; init; }
    public string? KeysPath { get; init; }
}

internal sealed class RateLimitSettings
{
    internal const string SectionName = "RateLimits";
    public int AuthSensitivePermitLimit { get; init; } = 10;
    public int TokenSetupPermitLimit { get; init; } = 20;
    public int UploadCreatePermitLimit { get; init; } = 4;
    public int WindowSeconds { get; init; } = 60;
}

internal sealed class ProductionOptionsValidator(
    IHostEnvironment environment,
    IOptions<FileStorageOptions> storage,
    IOptions<ImportOptions> imports) : IValidateOptions<DeploymentOptions>
{
    public ValidateOptionsResult Validate(string? name, DeploymentOptions options)
    {
        if (!IsProductionLike(environment))
            return ValidateOptionsResult.Success;
        if (!Enum.TryParse<DeploymentTopology>(options.Topology, true, out var topology))
            return Fail("Deployment:Topology is invalid.");
        if (!IsHttpsOrigin(options.PublicWebOrigin) || !IsHttpsOrigin(options.PublicApiOrigin))
            return Fail("Deployment public origins must be absolute HTTPS origins.");
        if (!Enum.TryParse<TrustedProxyMode>(options.TrustedProxyMode, true, out var proxyMode))
            return Fail("Deployment:TrustedProxyMode is invalid.");
        if (proxyMode == TrustedProxyMode.KNOWN_PROXIES && !options.KnownProxies.All(IsIpAddress))
            return Fail("Deployment:KnownProxies contains an invalid IP address.");
        if (proxyMode == TrustedProxyMode.KNOWN_NETWORKS && !options.KnownNetworks.All(IsNetwork))
            return Fail("Deployment:KnownNetworks contains an invalid CIDR network.");
        var origins = options.AllowedFrontendOrigins.Select(NormalizeOrigin).ToArray();
        if (origins.Any(origin => origin is null))
            return Fail("Deployment:AllowedFrontendOrigins contains an invalid origin.");
        if (origins.Distinct(StringComparer.OrdinalIgnoreCase).Count() != origins.Length)
            return Fail("Deployment:AllowedFrontendOrigins contains duplicate origins.");
        if (topology == DeploymentTopology.SPLIT_ORIGIN && origins.Length == 0)
            return Fail("Split-origin deployment requires exact allowed frontend origins.");
        if (topology == DeploymentTopology.SAME_ORIGIN && origins.Length > 0)
            return Fail("Same-origin deployment must not configure CORS frontend origins.");
        if (string.IsNullOrWhiteSpace(options.BuildVersion) || string.IsNullOrWhiteSpace(options.CommitSha))
            return Fail("Deployment build metadata is required.");
        if (string.Equals(storage.Value.Provider, "Local", StringComparison.OrdinalIgnoreCase))
            return Fail("FileStorage:Provider Local is not allowed outside Development.");
        if (IsUnsafeTemporaryDirectory(imports.Value.TemporaryWorkingDirectory, storage.Value.ResolvedLocalRootPath))
            return Fail("Imports:TemporaryWorkingDirectory is unsafe.");
        return ValidateOptionsResult.Success;
    }

    private static bool IsProductionLike(IHostEnvironment environment) => !environment.IsDevelopment() && !environment.IsEnvironment("Testing");
    private static ValidateOptionsResult Fail(string message) => ValidateOptionsResult.Fail(message);
    internal static string? NormalizeOrigin(string? value)
    {
        if (!IsHttpsOrigin(value))
            return null;
        return new Uri(value!, UriKind.Absolute).GetLeftPart(UriPartial.Authority);
    }
    internal static bool IsHttpsOrigin(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment) || uri.AbsolutePath != "/")
            return false;
        return !uri.IsLoopback && !uri.Host.EndsWith(".local", StringComparison.OrdinalIgnoreCase) && uri.Host != "localhost" && !uri.Host.Contains('*');
    }
    private static bool IsIpAddress(string value) => IPAddress.TryParse(value, out _);
    private static bool IsNetwork(string value)
    {
        var pieces = value.Split('/', 2);
        return pieces.Length == 2 && IPAddress.TryParse(pieces[0], out _) && int.TryParse(pieces[1], out var prefix) && prefix is >= 0 and <= 128;
    }
    private static bool IsUnsafeTemporaryDirectory(string? temporaryPath, string? storageRoot)
    {
        if (string.IsNullOrWhiteSpace(temporaryPath))
            return true;
        var temp = Path.GetFullPath(temporaryPath);
        return temp.Contains("wwwroot", StringComparison.OrdinalIgnoreCase)
            || temp.Contains("frontend" + Path.DirectorySeparatorChar + "public", StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrWhiteSpace(storageRoot) && string.Equals(temp, Path.GetFullPath(storageRoot), StringComparison.OrdinalIgnoreCase));
    }
}

internal sealed class DataProtectionSettingsValidator(IHostEnvironment environment, IOptions<ImportOptions> imports) : IValidateOptions<DataProtectionSettings>
{
    public ValidateOptionsResult Validate(string? name, DataProtectionSettings options)
    {
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
            return ValidateOptionsResult.Success;
        if (string.IsNullOrWhiteSpace(options.ApplicationName) || string.IsNullOrWhiteSpace(options.KeysPath))
            return ValidateOptionsResult.Fail("DataProtection application name and keys path are required.");
        var path = Path.GetFullPath(options.KeysPath);
        if (path.Contains("wwwroot", StringComparison.OrdinalIgnoreCase) || path.Contains(".git", StringComparison.OrdinalIgnoreCase) || string.Equals(path, Path.GetFullPath(imports.Value.TemporaryWorkingDirectory), StringComparison.OrdinalIgnoreCase))
            return ValidateOptionsResult.Fail("DataProtection keys path is unsafe.");
        return ValidateOptionsResult.Success;
    }
}

internal sealed class RateLimitSettingsValidator : IValidateOptions<RateLimitSettings>
{
    public ValidateOptionsResult Validate(string? name, RateLimitSettings options) => options.AuthSensitivePermitLimit > 0 && options.TokenSetupPermitLimit > 0 && options.UploadCreatePermitLimit > 0 && options.WindowSeconds > 0
        ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail("Rate limit values must be positive.");
}
