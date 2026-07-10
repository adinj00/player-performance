using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PlayerPerformance.Domain.Users;
using PlayerPerformance.Infrastructure.Identity;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.IntegrationTests;

public class TestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string inMemoryDatabaseName = $"player-performance-tests-{Guid.NewGuid():N}";

    protected virtual bool UseInMemoryDatabase => false;
    public TestApplicationFactory()
    {
        Environment.SetEnvironmentVariable("PlayerPerformance__ServiceName", "PlayerPerformance.Api.Tests");
        Environment.SetEnvironmentVariable("PlayerPerformance__FrontendOrigin", "https://frontend.test");
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            "Host=127.0.0.1;Port=1;Database=player_performance_tests;Username=test_user;Password=test_password;Timeout=1;Command Timeout=1");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
        {
            services.AddDataProtection().UseEphemeralDataProtectionProvider();
            if (UseInMemoryDatabase)
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<AppDbContext>();
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(inMemoryDatabaseName));
            }
        });
    }

    public async Task<ApplicationUser> CreateUserAsync(
        string email,
        string password,
        UserAccountStatus accountStatus = UserAccountStatus.ACTIVE,
        bool requiresPasswordChange = false)
    {
        await using var scope = Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            AccountStatus = accountStatus,
            RequiresPasswordChange = requiresPasswordChange,
            CreatedUtc = DateTimeOffset.UtcNow,
            UpdatedUtc = DateTimeOffset.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(error => error.Description)));
        return user;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Environment.SetEnvironmentVariable("PlayerPerformance__ServiceName", null);
            Environment.SetEnvironmentVariable("PlayerPerformance__FrontendOrigin", null);
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        }

        base.Dispose(disposing);
    }
}

public sealed class IdentityTestApplicationFactory : TestApplicationFactory
{
    protected override bool UseInMemoryDatabase => true;
}
