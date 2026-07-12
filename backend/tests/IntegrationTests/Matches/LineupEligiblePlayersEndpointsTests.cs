using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Domain.Players;
using PlayerPerformance.Domain.Staff;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.IntegrationTests.Matches;

public sealed class LineupEligiblePlayersEndpointsTests
{
    private static readonly DateTime Now = new(2026, 7, 12, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task EligiblePlayers_UsesMatchDateExcludesArchivedAndRequiresEditorRole()
    {
        using var factory = new IdentityTestApplicationFactory();
        var seed = await SeedAsync(factory);
        var admin = await factory.CreateUserAsync("admin.lineup@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(admin.Id, StaffRole.ADMIN);
        using var adminClient = CreateClient(factory);
        await LoginAsync(adminClient, admin.Email!);

        using var response = await adminClient.GetAsync($"/api/matches/{seed.MatchId}/lineup/eligible-players");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var ids = body.RootElement.EnumerateArray().Select(x => x.GetProperty("id").GetGuid()).ToHashSet();
        Assert.Contains(seed.HistoricalPlayerId, ids);
        Assert.Contains(seed.CurrentPlayerId, ids);
        Assert.Contains(seed.FuturePlayerId, ids);
        Assert.DoesNotContain(seed.OtherTeamPlayerId, ids);
        Assert.DoesNotContain(seed.ArchivedPlayerId, ids);

        var coach = await factory.CreateUserAsync("coach.lineup@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(coach.Id, StaffRole.COACH);
        using var coachClient = CreateClient(factory);
        await LoginAsync(coachClient, coach.Email!);
        using var denied = await coachClient.GetAsync($"/api/matches/{seed.MatchId}/lineup/eligible-players");
        Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);

        using var missing = await adminClient.GetAsync($"/api/matches/{Guid.NewGuid()}/lineup/eligible-players");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    private static async Task<Seed> SeedAsync(IdentityTestApplicationFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var teamId = Guid.NewGuid();
        var otherTeamId = Guid.NewGuid();
        var matchId = Guid.NewGuid();
        db.Teams.AddRange(
            Team.Create(teamId, "U19", "U19", TeamTrackingLevel.STANDARD, 1, Now),
            Team.Create(otherTeamId, "U17", "U17", TeamTrackingLevel.STANDARD, 2, Now));
        db.Matches.Add(Match.Create(matchId, Guid.NewGuid(), Guid.NewGuid(), teamId, Guid.NewGuid(), null, new DateTime(2026, 7, 1, 18, 0, 0, DateTimeKind.Utc), null, MatchLocationType.HOME, Now));

        var historical = Player.Create(Guid.NewGuid(), "Historijski", "Igrač", null, null, Now);
        var current = Player.Create(Guid.NewGuid(), "Trenutni", "Igrač", null, null, Now);
        var future = Player.Create(Guid.NewGuid(), "Budući", "Igrač", null, null, Now);
        var otherTeam = Player.Create(Guid.NewGuid(), "Druga", "Selekcija", null, null, Now);
        var archived = Player.Create(Guid.NewGuid(), "Arhiviran", "Igrač", null, null, Now);
        archived.Archive(Now);
        db.Players.AddRange(historical, current, future, otherTeam, archived);
        db.PlayerTeamAssignments.AddRange(
            PlayerTeamAssignment.Create(Guid.NewGuid(), historical.Id, teamId, new DateOnly(2026, 1, 1), new DateOnly(2026, 7, 1), Now),
            PlayerTeamAssignment.Create(Guid.NewGuid(), current.Id, teamId, new DateOnly(2026, 7, 1), null, Now),
            PlayerTeamAssignment.Create(Guid.NewGuid(), future.Id, teamId, new DateOnly(2026, 7, 1), new DateOnly(2026, 12, 31), Now),
            PlayerTeamAssignment.Create(Guid.NewGuid(), otherTeam.Id, otherTeamId, new DateOnly(2026, 1, 1), null, Now),
            PlayerTeamAssignment.Create(Guid.NewGuid(), archived.Id, teamId, new DateOnly(2026, 1, 1), null, Now));
        await db.SaveChangesAsync();
        return new(matchId, historical.Id, current.Id, future.Id, otherTeam.Id, archived.Id);
    }

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory) => factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
    private static async Task LoginAsync(HttpClient client, string email)
    {
        var csrf = await GetCsrfAsync(client);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login") { Content = JsonContent.Create(new { email, password = "Temporary!Pass123" }) };
        request.Headers.Add("X-CSRF-TOKEN", csrf);
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<string> GetCsrfAsync(HttpClient client)
    {
        using var response = await client.GetAsync("/api/auth/csrf");
        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return body.RootElement.GetProperty("requestToken").GetString()!;
    }

    private sealed record Seed(Guid MatchId, Guid HistoricalPlayerId, Guid CurrentPlayerId, Guid FuturePlayerId, Guid OtherTeamPlayerId, Guid ArchivedPlayerId);
}
