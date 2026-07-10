using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PlayerPerformance.IntegrationTests;

public sealed class TestApplicationFactory : WebApplicationFactory<Program>
{
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
        });
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
