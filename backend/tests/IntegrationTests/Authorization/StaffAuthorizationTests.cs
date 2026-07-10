using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Domain.Staff;
using PlayerPerformance.Infrastructure.Identity;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.IntegrationTests.Authorization;

public sealed class StaffAuthorizationTests : IClassFixture<IdentityTestApplicationFactory>
{
    private readonly IdentityTestApplicationFactory factory;

    public StaffAuthorizationTests(IdentityTestApplicationFactory factory) => this.factory = factory;

    [Fact]
    public async Task Policies_ShouldReturnUnauthorized_WhenCallerIsUnauthenticated()
    {
        using var client = CreateClient();
        using var response = await client.GetAsync("/_test/policies/admin");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Admin_ShouldSatisfyEveryStaffPolicy_AndSessionShouldExposeEffectivePermissions()
    {
        var client = CreateClient();
        var user = await factory.CreateUserAsync("admin.policy@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(user.Id, StaffRole.ADMIN);
        await LoginAsync(client, user.Email!);

        foreach (var path in new[] { "admin", "verify-reports", "import-data", "medical-details" })
        {
            using var response = await client.GetAsync($"/_test/policies/{path}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        using var session = await client.GetAsync("/api/auth/session");
        using var payload = await JsonDocument.ParseAsync(await session.Content.ReadAsStreamAsync());
        var userPayload = payload.RootElement.GetProperty("user");
        Assert.Equal("ADMIN", userPayload.GetProperty("primaryRole").GetString());
        var permissions = userPayload.GetProperty("permissions");
        Assert.True(permissions.GetProperty("canVerifyReports").GetBoolean());
        Assert.True(permissions.GetProperty("canImportData").GetBoolean());
        Assert.True(permissions.GetProperty("canViewMedicalDetails").GetBoolean());
        Assert.False(userPayload.TryGetProperty("teamIds", out _));
        Assert.False(userPayload.TryGetProperty("securityStamp", out _));
    }

    [Fact]
    public async Task NonAdminPermission_ShouldOnlySatisfyItsMatchingPolicy()
    {
        var client = CreateClient();
        var user = await factory.CreateUserAsync("analyst.policy@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(user.Id, StaffRole.ANALYST, canVerifyReports: true);
        await LoginAsync(client, user.Email!);

        using var allowed = await client.GetAsync("/_test/policies/verify-reports");
        using var denied = await client.GetAsync("/_test/policies/admin");
        using var otherDenied = await client.GetAsync("/_test/policies/import-data");
        Assert.Equal(HttpStatusCode.NoContent, allowed.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, otherDenied.StatusCode);
    }

    [Fact]
    public async Task MissingAccessProfile_ShouldFailClosed_AndExposeNoPrivilegedSessionValues()
    {
        var client = CreateClient();
        var user = await factory.CreateUserAsync("missing.profile@example.com", "Temporary!Pass123");
        await LoginAsync(client, user.Email!);

        using var denied = await client.GetAsync("/_test/policies/admin");
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
        using var session = await client.GetAsync("/api/auth/session");
        using var payload = await JsonDocument.ParseAsync(await session.Content.ReadAsStreamAsync());
        var userPayload = payload.RootElement.GetProperty("user");
        Assert.Equal(JsonValueKind.Null, userPayload.GetProperty("primaryRole").ValueKind);
        Assert.False(userPayload.GetProperty("permissions").GetProperty("canImportData").GetBoolean());
    }

    [Fact]
    public async Task FirstAdminRoleHandoff_ShouldBeIdempotent_AndFailWhenFallbackIsAmbiguous()
    {
        using var singleUserFactory = new IdentityTestApplicationFactory();
        var firstUser = await singleUserFactory.CreateUserAsync("handoff@example.com", "Temporary!Pass123");
        await using (var scope = singleUserFactory.Services.CreateAsyncScope())
        {
            var handoff = CreateFallbackHandoff(scope.ServiceProvider);
            await handoff.ApplyAsync();
            await handoff.ApplyAsync();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var profile = await dbContext.StaffAccessProfiles.SingleAsync(profile => profile.UserId == firstUser.Id);
            Assert.Equal(StaffRole.ADMIN, profile.PrimaryRole);
            Assert.Equal(1, await dbContext.StaffAccessProfiles.CountAsync());
        }

        using var ambiguousFactory = new IdentityTestApplicationFactory();
        await ambiguousFactory.CreateUserAsync("first.ambiguous@example.com", "Temporary!Pass123");
        await ambiguousFactory.CreateUserAsync("second.ambiguous@example.com", "Temporary!Pass123");
        await using var ambiguousScope = ambiguousFactory.Services.CreateAsyncScope();
        var ambiguousHandoff = CreateFallbackHandoff(ambiguousScope.ServiceProvider);
        await Assert.ThrowsAsync<InvalidOperationException>(() => ambiguousHandoff.ApplyAsync());
    }

    private HttpClient CreateClient() => factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    private static FirstAdminRoleHandoff CreateFallbackHandoff(IServiceProvider services) => new(
        services.GetRequiredService<AppDbContext>(),
        Options.Create(new FirstAdminBootstrapOptions()),
        services.GetRequiredService<ISystemClock>());

    private static async Task LoginAsync(HttpClient client, string email)
    {
        using var csrf = await client.GetAsync("/api/auth/csrf");
        using var csrfPayload = await JsonDocument.ParseAsync(await csrf.Content.ReadAsStreamAsync());
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { email, password = "Temporary!Pass123" })
        };
        request.Headers.Add("X-CSRF-TOKEN", csrfPayload.RootElement.GetProperty("requestToken").GetString());
        using var login = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }
}
