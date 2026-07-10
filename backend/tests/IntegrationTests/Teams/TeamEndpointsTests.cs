using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlayerPerformance.Domain.Staff;
using PlayerPerformance.Infrastructure.Persistence;
using PlayerPerformance.Infrastructure.Teams;

namespace PlayerPerformance.IntegrationTests.Teams;

public sealed class TeamEndpointsTests
{
    [Fact]
    public async Task Initializer_ShouldSeedExactlyTheDefaultTeamsOnlyWhenEmpty()
    {
        using var factory = new IdentityTestApplicationFactory();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var initializer = scope.ServiceProvider.GetRequiredService<TeamStartupInitializer>();
            await initializer.InitializeAsync();
            await initializer.InitializeAsync();
            var teams = await scope.ServiceProvider.GetRequiredService<AppDbContext>().Teams.OrderBy(x => x.DisplayOrder).ToListAsync();
            Assert.Equal(["First Team", "U19", "U17", "U15", "U13", "U11"], teams.Select(x => x.Name));
        }
    }

    [Fact]
    public async Task TeamEndpoints_ShouldEnforceAdminAndSupportCreateArchiveAndListFiltering()
    {
        using var factory = new IdentityTestApplicationFactory();
        using var anonymous = CreateClient(factory);
        using var unauthorized = await anonymous.GetAsync("/api/settings/teams");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        var nonAdmin = await factory.CreateUserAsync("coach.teams@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(nonAdmin.Id, StaffRole.COACH);
        using var nonAdminClient = CreateClient(factory);
        await LoginAsync(nonAdminClient, nonAdmin.Email!);
        using var forbidden = await nonAdminClient.GetAsync("/api/settings/teams");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        var admin = await factory.CreateUserAsync("admin.teams@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(admin.Id, StaffRole.ADMIN);
        using var adminClient = CreateClient(factory);
        var csrf = await GetCsrfAsync(adminClient);
        await LoginAsync(adminClient, admin.Email!, csrf);
        csrf = await GetCsrfAsync(adminClient);
        using var create = await SendJsonAsync(adminClient, HttpMethod.Post, "/api/settings/teams", new
        {
            name = "  U19  ",
            trackingLevel = "STANDARD"
        }, csrf);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        using var body = await JsonDocument.ParseAsync(await create.Content.ReadAsStreamAsync());
        var id = body.RootElement.GetProperty("id").GetGuid();
        using var archive = await SendAsync(adminClient, HttpMethod.Post, $"/api/settings/teams/{id}/archive", csrf);
        Assert.True(archive.StatusCode == HttpStatusCode.OK, await archive.Content.ReadAsStringAsync());
        using var defaultList = await adminClient.GetAsync("/api/settings/teams");
        Assert.DoesNotContain("U19", await defaultList.Content.ReadAsStringAsync());
        using var allList = await adminClient.GetAsync("/api/settings/teams?includeArchived=true");
        Assert.Contains("ARCHIVED", await allList.Content.ReadAsStringAsync());
    }

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory) => factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
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
    private static async Task LoginAsync(HttpClient client, string email, string? csrf = null)
    {
        csrf ??= await GetCsrfAsync(client);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login") { Content = JsonContent.Create(new { email, password = "Temporary!Pass123" }) };
        request.Headers.Add("X-CSRF-TOKEN", csrf);
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
