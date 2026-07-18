using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlayerPerformance.Domain.Settings;
using PlayerPerformance.Domain.Staff;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.IntegrationTests;

public sealed class DashboardEndpointsTests
{
    [Fact]
    public async Task ContextOptions_ShouldRequireAuthentication_AndExcludeArchivedTeams()
    {
        using var factory = new IdentityTestApplicationFactory();
        var (activeTeamId, _) = await SeedContextAsync(factory);
        using var anonymous = CreateClient(factory);
        using var unauthorized = await anonymous.GetAsync("/api/dashboard/context-options");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        var coach = await factory.CreateUserAsync("dashboard.context@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(coach.Id, StaffRole.COACH);
        using var client = CreateClient(factory);
        await LoginAsync(client, coach.Email!);

        using var response = await client.GetAsync("/api/dashboard/context-options");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var teams = json.RootElement.GetProperty("teams").EnumerateArray().ToArray();
        Assert.Contains(teams, team => team.GetProperty("id").GetGuid() == activeTeamId);
        Assert.DoesNotContain(teams, team => team.GetProperty("status").GetString() == "ARCHIVED");
    }

    [Fact]
    public async Task Overview_ShouldRequireExplicitContext_AndKeepFinalOnlyWorkflowPrivate()
    {
        using var factory = new IdentityTestApplicationFactory();
        var (teamId, seasonId) = await SeedContextAsync(factory);
        var coach = await factory.CreateUserAsync("dashboard.overview@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(coach.Id, StaffRole.COACH);
        using var client = CreateClient(factory);
        await LoginAsync(client, coach.Email!);

        using var invalid = await client.GetAsync("/api/dashboard/overview");
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        using var response = await client.GetAsync($"/api/dashboard/overview?teamId={teamId}&seasonId={seasonId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(payload);
        var workflow = json.RootElement.GetProperty("reportWorkflow");
        Assert.Equal("FINAL_ONLY", workflow.GetProperty("visibilityMode").GetString());
        Assert.False(workflow.TryGetProperty("playedMatchCount", out _));
        Assert.False(workflow.TryGetProperty("missingReportCount", out _));
        Assert.DoesNotContain("injury", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("diagnosis", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("restrictedNotes", payload, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<(Guid TeamId, Guid SeasonId)> SeedContextAsync(TestApplicationFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTime.UtcNow;
        var active = Team.Create(Guid.NewGuid(), $"Dashboard Active {Guid.NewGuid():N}", $"DASHBOARD ACTIVE {Guid.NewGuid():N}", TeamTrackingLevel.BASIC, 70, now);
        var archived = Team.Create(Guid.NewGuid(), $"Dashboard Archived {Guid.NewGuid():N}", $"DASHBOARD ARCHIVED {Guid.NewGuid():N}", TeamTrackingLevel.BASIC, 71, now);
        archived.Archive(now);
        var season = Season.Create(Guid.NewGuid(), "Dashboard 2026", "DASHBOARD 2026", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), now);
        db.AddRange(active, archived, season);
        await db.SaveChangesAsync();
        return (active.Id, season.Id);
    }

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory) => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost")
    });

    private static async Task LoginAsync(HttpClient client, string email)
    {
        using var csrf = await client.GetAsync("/api/auth/csrf");
        using var json = await JsonDocument.ParseAsync(await csrf.Content.ReadAsStreamAsync());
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login") { Content = JsonContent.Create(new { email, password = "Temporary!Pass123" }) };
        request.Headers.Add("X-CSRF-TOKEN", json.RootElement.GetProperty("requestToken").GetString());
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
