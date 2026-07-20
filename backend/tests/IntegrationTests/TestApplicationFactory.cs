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
using PlayerPerformance.Infrastructure.Staff;
using PlayerPerformance.Domain.Staff;

namespace PlayerPerformance.IntegrationTests;

public class TestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string inMemoryDatabaseName = $"player-performance-tests-{Guid.NewGuid():N}";
    private readonly string fileStorageRoot = Path.Combine(Path.GetTempPath(), "player-performance-integration-storage", Guid.NewGuid().ToString("N"));

    protected virtual bool UseInMemoryDatabase => false;
    public TestApplicationFactory()
    {
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("PlayerPerformance:ServiceName", "PlayerPerformance.Api.Tests");
        builder.UseSetting("PlayerPerformance:FrontendOrigin", "https://frontend.test");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=127.0.0.1;Port=1;Database=player_performance_tests;Username=test_user;Password=test_password;Timeout=1;Command Timeout=1");
        builder.UseSetting("FileStorage:Provider", "Local");
        builder.UseSetting("FileStorage:LocalRootPath", fileStorageRoot);
        builder.UseSetting("FileStorage:MaxObjectSizeBytes", "1024");
        builder.UseSetting("Media:MaxUploadSizeBytes", "1024");
        builder.UseSetting("Imports:MaxUploadSizeBytes", "1024");
        builder.UseSetting("Imports:MaxXlsxUncompressedSizeBytes", "20480");
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

    public async Task CreateAccessProfileAsync(
        Guid userId,
        StaffRole role,
        bool canVerifyReports = false,
        bool canImportData = false,
        bool canViewMedicalDetails = false)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.StaffAccessProfiles.Add(new StaffAccessProfile
        {
            UserId = userId,
            PrimaryRole = role,
            CanVerifyReports = canVerifyReports,
            CanImportData = canImportData,
            CanViewMedicalDetails = canViewMedicalDetails,
            CreatedUtc = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (Directory.Exists(fileStorageRoot))
            {
                Directory.Delete(fileStorageRoot, true);
            }
        }

        base.Dispose(disposing);
    }
}

public sealed class IdentityTestApplicationFactory : TestApplicationFactory
{
    protected override bool UseInMemoryDatabase => true;
}
