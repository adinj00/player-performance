using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace PlayerPerformance.Infrastructure.Identity;

public static class FirstAdminBootstrapServiceProviderExtensions
{
    public static async Task BootstrapFirstAdminAsync(
        this IServiceProvider services,
        IWebHostEnvironment environment,
        CancellationToken cancellationToken = default)
    {
        if (string.Equals(environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var bootstrapper = scope.ServiceProvider.GetRequiredService<FirstAdminBootstrapper>();
        await bootstrapper.BootstrapAsync(cancellationToken);
        var handoff = scope.ServiceProvider.GetRequiredService<FirstAdminRoleHandoff>();
        await handoff.ApplyAsync(cancellationToken);
    }
}
