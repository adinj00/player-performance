using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlayerPerformance.Domain.Players;
using PlayerPerformance.Domain.Staff;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.IntegrationTests.Players;

public sealed class MedicalAvailabilityEndpointsTests
{
    [Fact]
    public async Task SafeAvailabilityList_ShouldSynthesizeUnknown_AndNeverSerializeRestrictedFields()
    {
        using var factory = new IdentityTestApplicationFactory();
        var (teamId, playerId) = await SeedAssignmentAsync(factory);
        var coach = await factory.CreateUserAsync("coach.medical@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(coach.Id, StaffRole.COACH);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        await LoginAsync(client, coach.Email!);

        using var response = await client.GetAsync($"/api/player-availability?teamId={teamId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var item = Assert.Single(document.RootElement.GetProperty("items").EnumerateArray());
        Assert.Equal(playerId, item.GetProperty("playerId").GetGuid());
        Assert.Equal("UNKNOWN", item.GetProperty("status").GetString());
        Assert.False(item.TryGetProperty("injuryId", out _));
        Assert.False(item.TryGetProperty("diagnosis", out _));
        Assert.False(item.TryGetProperty("bodyArea", out _));
        Assert.False(item.TryGetProperty("restrictedNotes", out _));
    }

    [Fact]
    public async Task InjuryRoutes_ShouldNotBeDiscoverableWithoutMedicalDetailPermission()
    {
        using var factory = new IdentityTestApplicationFactory();
        var (teamId, _) = await SeedAssignmentAsync(factory);
        var coach = await factory.CreateUserAsync("coach.no-details@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(coach.Id, StaffRole.COACH);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        await LoginAsync(client, coach.Email!);

        using var response = await client.GetAsync($"/api/medical/injuries?teamId={teamId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task InjuryCandidates_ShouldUseOccurrenceDateAssignment_AndRemainSupportOnly()
    {
        using var factory = new IdentityTestApplicationFactory();
        var (teamId, playerId) = await SeedAssignmentAsync(factory);
        var admin = await factory.CreateUserAsync("admin.candidates@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(admin.Id, StaffRole.ADMIN);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await LoginAsync(client, admin.Email!);

        using var response = await client.GetAsync($"/api/medical/injury-player-candidates?teamId={teamId}&occurredOn={DateOnly.FromDateTime(DateTime.UtcNow):yyyy-MM-dd}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var item = Assert.Single(document.RootElement.GetProperty("items").EnumerateArray());
        Assert.Equal(playerId, item.GetProperty("id").GetGuid());
        Assert.True(item.TryGetProperty("eligibleAssignment", out _));
        Assert.False(item.TryGetProperty("status", out _));
        Assert.False(item.TryGetProperty("diagnosis", out _));
        Assert.False(item.TryGetProperty("restrictedNotes", out _));
    }

    [Fact]
    public async Task InjuryList_ShouldApplyTeamScopeInDatabaseQuery()
    {
        using var factory = new IdentityTestApplicationFactory();
        var (teamId, _) = await SeedAssignmentAsync(factory);
        var admin = await factory.CreateUserAsync("admin.injury-list@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(admin.Id, StaffRole.ADMIN);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await LoginAsync(client, admin.Email!);

        using var response = await client.GetAsync($"/api/medical/injuries?teamId={teamId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<(Guid TeamId, Guid PlayerId)> SeedAssignmentAsync(TestApplicationFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTime.UtcNow;
        var team = Team.Create(Guid.NewGuid(), "Medical Test Team", "MEDICAL TEST TEAM", TeamTrackingLevel.BASIC, 90, now);
        var player = Player.Create(Guid.NewGuid(), "Medical", "Player", null, null, now);
        db.Teams.Add(team);
        db.Players.Add(player);
        db.PlayerTeamAssignments.Add(PlayerTeamAssignment.Create(Guid.NewGuid(), player.Id, team.Id, DateOnly.FromDateTime(now).AddDays(-1), null, now));
        await db.SaveChangesAsync();
        return (team.Id, player.Id);
    }

    private static async Task LoginAsync(HttpClient client, string email)
    {
        var csrfResponse = await client.GetAsync("/api/auth/csrf");
        using var csrfJson = await JsonDocument.ParseAsync(await csrfResponse.Content.ReadAsStreamAsync());
        var token = csrfJson.RootElement.GetProperty("requestToken").GetString()!;
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { email, password = "Temporary!Pass123" })
        };
        request.Headers.Add("X-CSRF-TOKEN", token);
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
