using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PlayerPerformance.Domain.Staff;

namespace PlayerPerformance.IntegrationTests.Settings;

public sealed class VenueAndOpponentEndpointsTests
{
    [Fact]
    public async Task Endpoints_ShouldRequireAdminAndSupportIndependentArchiveFiltering()
    {
        using var factory = new IdentityTestApplicationFactory();
        using var anonymousClient = CreateClient(factory);
        using var unauthorized = await anonymousClient.GetAsync("/api/settings/venues");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        var coach = await factory.CreateUserAsync("coach.settings@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(coach.Id, StaffRole.COACH);
        using var coachClient = CreateClient(factory);
        await LoginAsync(coachClient, coach.Email!);
        using var forbidden = await coachClient.GetAsync("/api/settings/opponents");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        var admin = await factory.CreateUserAsync("admin.settings@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(admin.Id, StaffRole.ADMIN);
        using var adminClient = CreateClient(factory);
        await LoginAsync(adminClient, admin.Email!);
        var csrf = await GetCsrfAsync(adminClient);

        using var venueCreate = await SendJsonAsync(adminClient, HttpMethod.Post, "/api/settings/venues", new
        {
            name = "  Stadion  "
        }, csrf);
        Assert.Equal(HttpStatusCode.Created, venueCreate.StatusCode);
        var venueId = await GetIdAsync(venueCreate);
        using var opponentCreate = await SendJsonAsync(adminClient, HttpMethod.Post, "/api/settings/opponents", new
        {
            name = "Stadion"
        }, csrf);
        Assert.Equal(HttpStatusCode.Created, opponentCreate.StatusCode);
        var opponentId = await GetIdAsync(opponentCreate);
        using var opponentUpdate = await SendJsonAsync(adminClient, HttpMethod.Patch, $"/api/settings/opponents/{opponentId}", new
        {
            name = "FK Stadium"
        }, csrf);
        Assert.Equal(HttpStatusCode.OK, opponentUpdate.StatusCode);
        using var opponentGet = await adminClient.GetAsync($"/api/settings/opponents/{opponentId}");
        Assert.Equal(HttpStatusCode.OK, opponentGet.StatusCode);
        using var archive = await SendAsync(adminClient, HttpMethod.Post, $"/api/settings/venues/{venueId}/archive", csrf);
        Assert.Equal(HttpStatusCode.OK, archive.StatusCode);
        using var archivedUpdate = await SendJsonAsync(adminClient, HttpMethod.Patch, $"/api/settings/venues/{venueId}", new
        {
            name = "New Stadion"
        }, csrf);
        Assert.Equal(HttpStatusCode.Conflict, archivedUpdate.StatusCode);
        using var activeVenues = await adminClient.GetAsync("/api/settings/venues");
        Assert.DoesNotContain("Stadion", await activeVenues.Content.ReadAsStringAsync());
        using var allVenues = await adminClient.GetAsync("/api/settings/venues?includeArchived=true");
        Assert.Contains("Stadion", await allVenues.Content.ReadAsStringAsync());
        using var restore = await SendAsync(adminClient, HttpMethod.Post, $"/api/settings/venues/{venueId}/restore", csrf);
        Assert.Equal(HttpStatusCode.OK, restore.StatusCode);
        Assert.Contains("\"isArchived\":false", await restore.Content.ReadAsStringAsync());
        using var duplicateVenue = await SendJsonAsync(adminClient, HttpMethod.Post, "/api/settings/venues", new
        {
            name = "stadion"
        }, csrf);
        Assert.Equal(HttpStatusCode.Conflict, duplicateVenue.StatusCode);
    }

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory) => factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
    private static async Task<Guid> GetIdAsync(HttpResponseMessage response)
    {
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return document.RootElement.GetProperty("id").GetGuid();
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
