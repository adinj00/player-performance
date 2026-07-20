using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using PlayerPerformance.Infrastructure.Files;
using PlayerPerformance.Infrastructure.Imports;

namespace PlayerPerformance.Infrastructure.Persistence;

public sealed class PendingMigrationsHealthCheck(AppDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var pending = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        return pending.Any() ? HealthCheckResult.Unhealthy("Required database migrations are pending.") : HealthCheckResult.Healthy();
    }
}

public sealed class StorageReadinessHealthCheck(IOptions<FileStorageOptions> options) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var root = options.Value.ResolvedLocalRootPath;
        return Task.FromResult(!string.IsNullOrWhiteSpace(root) && Directory.Exists(root)
            ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("Configured storage is unavailable."));
    }
}

public sealed class ImportTemporaryDirectoryHealthCheck(IOptions<ImportOptions> options) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            Directory.CreateDirectory(options.Value.TemporaryWorkingDirectory);
            var probe = Path.Combine(options.Value.TemporaryWorkingDirectory, $".health-{Guid.NewGuid():N}");
            using (File.Create(probe))
            { }
            File.Delete(probe);
            return Task.FromResult(HealthCheckResult.Healthy());
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Import temporary directory is unavailable."));
        }
    }
}
