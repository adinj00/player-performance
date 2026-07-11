using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PlayerPerformance.Domain.Staff;

namespace PlayerPerformance.IntegrationTests.Players;

public sealed class PlayerEndpointsTests
{
    [Fact]
    public async Task PlayerEndpoints_ShouldEnforceAdminAndSupportRecordLifecycle()
    {
        using var factory = new IdentityTestApplicationFactory();
        using var anonymous = CreateClient(factory);
        using var unauthorized = await anonymous.GetAsync("/api/players");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
        var coach = await factory.CreateUserAsync("coach.players@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(coach.Id, StaffRole.COACH);
        using var coachClient = CreateClient(factory);
        await LoginAsync(coachClient, coach.Email!);
        using var permittedRead = await coachClient.GetAsync("/api/players");
        Assert.Equal(HttpStatusCode.OK, permittedRead.StatusCode);
        var admin = await factory.CreateUserAsync("admin.players@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(admin.Id, StaffRole.ADMIN);
        using var client = CreateClient(factory);
        await LoginAsync(client, admin.Email!);
        var csrf = await GetCsrfAsync(client);
        using var created = await SendJsonAsync(client, HttpMethod.Post, "/api/players", new { firstName = "  Amar  ", lastName = "  Kovač  ", preferredName = " Aki ", dateOfBirth = "2005-02-03" }, csrf);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var playerId = await GetIdAsync(created);
        using var duplicate = await SendJsonAsync(client, HttpMethod.Post, "/api/players", new { firstName = "Amar", lastName = "Kovač", dateOfBirth = "2005-02-03" }, csrf);
        Assert.Equal(HttpStatusCode.Created, duplicate.StatusCode);
        using var paged = await client.GetAsync("/api/players?page=1&pageSize=1");
        using var pagedBody = await JsonDocument.ParseAsync(await paged.Content.ReadAsStreamAsync());
        Assert.Equal(1, pagedBody.RootElement.GetProperty("items").GetArrayLength());
        Assert.Equal(2, pagedBody.RootElement.GetProperty("totalCount").GetInt32());
        using var searched = await client.GetAsync("/api/players?search=aki&page=1&pageSize=25");
        var searchBody = await searched.Content.ReadAsStringAsync();
        Assert.True(searched.StatusCode == HttpStatusCode.OK, searchBody);
        Assert.Contains("Aki", searchBody);
        using var deactivate = await SendAsync(client, HttpMethod.Post, $"/api/players/{playerId}/deactivate", csrf);
        Assert.Equal(HttpStatusCode.OK, deactivate.StatusCode);
        using var archive = await SendAsync(client, HttpMethod.Post, $"/api/players/{playerId}/archive", csrf);
        Assert.Equal(HttpStatusCode.OK, archive.StatusCode);
        using var defaultList = await client.GetAsync("/api/players");
        Assert.DoesNotContain(playerId.ToString(), await defaultList.Content.ReadAsStringAsync());
        using var archived = await client.GetAsync("/api/players?includeArchived=true&status=ARCHIVED");
        Assert.Contains(playerId.ToString(), await archived.Content.ReadAsStringAsync());
        using var updateArchived = await SendJsonAsync(client, HttpMethod.Patch, $"/api/players/{playerId}", new { firstName = "Amar", lastName = "Kovač" }, csrf);
        Assert.Equal(HttpStatusCode.Conflict, updateArchived.StatusCode);
        using var restore = await SendAsync(client, HttpMethod.Post, $"/api/players/{playerId}/restore", csrf);
        Assert.Contains("INACTIVE", await restore.Content.ReadAsStringAsync());
        using var activate = await SendAsync(client, HttpMethod.Post, $"/api/players/{playerId}/activate", csrf);
        Assert.Contains("ACTIVE", await activate.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task PlayerEndpoints_ShouldReturnNotFoundValidationAndNoDeleteRoute()
    {
        using var factory = new IdentityTestApplicationFactory();
        var admin = await factory.CreateUserAsync("admin.players.errors@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(admin.Id, StaffRole.ADMIN);
        using var client = CreateClient(factory);
        await LoginAsync(client, admin.Email!);
        var csrf = await GetCsrfAsync(client);
        using var missing = await client.GetAsync($"/api/players/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        using var invalid = await SendJsonAsync(client, HttpMethod.Post, "/api/players", new { firstName = " ", lastName = "Kovač", dateOfBirth = "2999-01-01" }, csrf);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalid.StatusCode);
        using var delete = await SendAsync(client, HttpMethod.Delete, $"/api/players/{Guid.NewGuid()}", csrf);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, delete.StatusCode);
    }

    [Fact]
    public async Task AssignmentEndpoints_ShouldCreateListEndAndRejectSameTeamOverlap()
    {
        using var factory = new IdentityTestApplicationFactory();
        var admin = await factory.CreateUserAsync("admin.assignments@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(admin.Id, StaffRole.ADMIN);
        using var client = CreateClient(factory);
        await LoginAsync(client, admin.Email!);
        var csrf = await GetCsrfAsync(client);
        using var teamResponse = await SendJsonAsync(client, HttpMethod.Post, "/api/settings/teams", new { name = "U19 Assignment", trackingLevel = "STANDARD" }, csrf);
        Assert.Equal(HttpStatusCode.Created, teamResponse.StatusCode);
        var teamId = await GetIdAsync(teamResponse);
        using var playerResponse = await SendJsonAsync(client, HttpMethod.Post, "/api/players", new { firstName = "Amar", lastName = "Assignment" }, csrf);
        var playerId = await GetIdAsync(playerResponse);
        using var created = await SendJsonAsync(client, HttpMethod.Post, $"/api/players/{playerId}/assignments", new { teamId, startDate = "2026-01-01" }, csrf);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var assignmentId = await GetIdAsync(created);
        using var overlap = await SendJsonAsync(client, HttpMethod.Post, $"/api/players/{playerId}/assignments", new { teamId, startDate = "2026-07-01" }, csrf);
        Assert.Equal(HttpStatusCode.Conflict, overlap.StatusCode);
        using var assignments = await client.GetAsync($"/api/players/{playerId}/assignments");
        Assert.Contains("CURRENT", await assignments.Content.ReadAsStringAsync());
        using var ended = await SendJsonAsync(client, HttpMethod.Post, $"/api/players/{playerId}/assignments/{assignmentId}/end", new { endDate = "2026-07-11" }, csrf);
        Assert.Equal(HttpStatusCode.OK, ended.StatusCode);
        using var repeated = await SendJsonAsync(client, HttpMethod.Post, $"/api/players/{playerId}/assignments/{assignmentId}/end", new { endDate = "2026-07-11" }, csrf);
        Assert.Equal(HttpStatusCode.Conflict, repeated.StatusCode);
    }

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory) => factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
    private static async Task<Guid> GetIdAsync(HttpResponseMessage response)
    {
        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return body.RootElement.GetProperty("id").GetGuid();
    }
    private static async Task<HttpResponseMessage> SendJsonAsync(HttpClient client, HttpMethod method, string path, object payload, string csrf)
    {
        var request = new HttpRequestMessage(method, path) { Content = JsonContent.Create(payload) };
        request.Headers.Add("X-CSRF-TOKEN", csrf);
        return await client.SendAsync(request);
    }
    private static async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpMethod method, string path, string csrf)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-CSRF-TOKEN", csrf);
        return await client.SendAsync(request);
    }
    private static async Task<string> GetCsrfAsync(HttpClient client)
    {
        using var response = await client.GetAsync("/api/auth/csrf");
        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return body.RootElement.GetProperty("requestToken").GetString()!;
    }
    private static async Task LoginAsync(HttpClient client, string email)
    {
        var csrf = await GetCsrfAsync(client);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login") { Content = JsonContent.Create(new { email, password = "Temporary!Pass123" }) };
        request.Headers.Add("X-CSRF-TOKEN", csrf);
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
