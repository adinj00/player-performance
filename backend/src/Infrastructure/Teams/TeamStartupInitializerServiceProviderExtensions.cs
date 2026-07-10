using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace PlayerPerformance.Infrastructure.Teams;

public static class TeamStartupInitializerServiceProviderExtensions
{
    public static async Task InitializeTeamsAsync(this IServiceProvider services, IWebHostEnvironment environment, CancellationToken ct = default)
    {
        if (string.Equals(environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase)) return;
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<TeamStartupInitializer>().InitializeAsync(ct);
    }
}
