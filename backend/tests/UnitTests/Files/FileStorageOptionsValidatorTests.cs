using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PlayerPerformance.Infrastructure.Files;

namespace PlayerPerformance.UnitTests.Files;

public sealed class FileStorageOptionsValidatorTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "player-performance-options", Guid.NewGuid().ToString("N"));

    [Fact]
    public void Validate_ShouldAcceptDevelopmentLocalAndNormalizeRelativeRoot()
    {
        var result = new FileStorageOptionsValidator(new Host("Development", root)).Validate(null, Options("storage"));

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(null, "storage", 1)]
    [InlineData("Unknown", "storage", 1)]
    [InlineData("Local", null, 1)]
    [InlineData("Local", "storage", 0)]
    public void Validate_ShouldRejectInvalidConfiguration(string? provider, string? localRoot, long maximum)
    {
        var result = new FileStorageOptionsValidator(new Host("Development", root)).Validate(null, new()
        {
            Provider = provider,
            LocalRootPath = localRoot,
            MaxObjectSizeBytes = maximum
        });

        Assert.True(result.Failed);
    }

    [Fact]
    public void Validate_ShouldRejectLocalOutsideDevelopmentAndPublicDirectories()
    {
        var production = new FileStorageOptionsValidator(new Host("Production", root)).Validate(null, Options("storage"));
        var publicRoot = new FileStorageOptionsValidator(new Host("Development", root)).Validate(null, Options("wwwroot/storage"));

        Assert.True(production.Failed);
        Assert.True(publicRoot.Failed);
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
            Directory.Delete(root, true);
    }
    private static FileStorageOptions Options(string path) => new()
    {
        Provider = "Local",
        LocalRootPath = path,
        MaxObjectSizeBytes = 1
    };
    private sealed class Host(string environmentName, string contentRoot) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = contentRoot;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
