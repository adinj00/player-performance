using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using PlayerPerformance.Api.Configuration;
using PlayerPerformance.Infrastructure.Files;
using PlayerPerformance.Infrastructure.Imports;

namespace PlayerPerformance.IntegrationTests.Configuration;

public sealed class ProductionOptionsValidatorTests
{
    [Fact]
    public void Validate_ShouldAcceptValidSplitOriginProfile()
    {
        var result = CreateValidator().Validate(null, ValidOptions("SPLIT_ORIGIN", ["https://app.example.ba"]));
        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("http://performance.example.ba")]
    [InlineData("https://localhost")]
    [InlineData("https://*.example.ba")]
    public void Validate_ShouldRejectUnsafePublicOrigin(string origin)
    {
        var result = CreateValidator().Validate(null, ValidOptions(publicWebOrigin: origin));
        Assert.False(result.Succeeded);
        Assert.DoesNotContain(origin, string.Join(' ', result.Failures!));
    }

    [Fact]
    public void Validate_ShouldRejectSplitOriginWithoutAllowedOrigin()
    {
        var result = CreateValidator().Validate(null, ValidOptions("SPLIT_ORIGIN", []));
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void DataProtectionValidator_ShouldRejectMissingPersistentSettings()
    {
        var validator = new DataProtectionSettingsValidator(new ProductionEnvironment(), Options.Create(new ImportOptions()));
        var result = validator.Validate(null, new DataProtectionSettings());
        Assert.False(result.Succeeded);
    }

    [Theory]
    [InlineData(0, 20, 4, 60)]
    [InlineData(10, 0, 4, 60)]
    [InlineData(10, 20, 0, 60)]
    public void RateLimits_ShouldRejectNonPositiveValues(int auth, int token, int upload, int seconds)
    {
        var result = new RateLimitSettingsValidator().Validate(null, new RateLimitSettings
        {
            AuthSensitivePermitLimit = auth,
            TokenSetupPermitLimit = token,
            UploadCreatePermitLimit = upload,
            WindowSeconds = seconds
        });
        Assert.False(result.Succeeded);
    }

    private static ProductionOptionsValidator CreateValidator() => new(new ProductionEnvironment(), Options.Create(new FileStorageOptions
    {
        Provider = "ApprovedProvider",
        MaxObjectSizeBytes = 1,
        ResolvedLocalRootPath = Path.Combine(Path.GetTempPath(), "storage")
    }), Options.Create(new ImportOptions
    {
        TemporaryWorkingDirectory = Path.Combine(Path.GetTempPath(), "imports")
    }));
    private static DeploymentOptions ValidOptions(string topology = "SAME_ORIGIN", string[]? allowedOrigins = null, string publicWebOrigin = "https://performance.example.ba") => new()
    {
        Topology = topology,
        PublicWebOrigin = publicWebOrigin,
        PublicApiOrigin = "https://performance.example.ba",
        AllowedFrontendOrigins = allowedOrigins ?? [],
        TrustedProxyMode = "DIRECT",
        BuildVersion = "test",
        CommitSha = "abc"
    };

    private sealed class ProductionEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
