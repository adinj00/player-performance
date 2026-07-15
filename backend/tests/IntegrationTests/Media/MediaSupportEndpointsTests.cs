using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PlayerPerformance.Domain.Staff;

namespace PlayerPerformance.IntegrationTests.Media;

public sealed class MediaSupportEndpointsTests
{
    [Fact]
    public async Task Capabilities_ShouldRequireAuthenticationAndExposeOnlyUploadRules()
    {
        using var factory = new IdentityTestApplicationFactory();
        using var anonymous = Client(factory);
        using var unauthorized = await anonymous.GetAsync("/api/media/capabilities");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        var admin = await factory.CreateUserAsync("media.support@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(admin.Id, StaffRole.ADMIN);
        using var client = Client(factory);
        await Login(client, admin.Email!);
        using var response = await client.GetAsync("/api/media/capabilities");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.True(body.RootElement.GetProperty("maxUploadSizeBytes").GetInt64() > 0);
        Assert.Contains(body.RootElement.GetProperty("uploadedFileTypes").EnumerateArray(), item => item.GetProperty("category").GetString() == "VIDEO");
        Assert.False(body.RootElement.TryGetProperty("storageKey", out _));
    }

    [Fact]
    public async Task LinkCandidates_ShouldReturnNotFoundForUnknownMedia()
    {
        using var factory = new IdentityTestApplicationFactory();
        var admin = await factory.CreateUserAsync("media.candidates@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(admin.Id, StaffRole.ADMIN);
        using var client = Client(factory);
        await Login(client, admin.Email!);
        using var response = await client.GetAsync($"/api/media/{Guid.NewGuid()}/link-candidates?targetType=MATCH&page=1&pageSize=25");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateAsset_ShouldAllowAnActiveAdministratorForAnActiveTeam()
    {
        using var factory = new IdentityTestApplicationFactory();
        var admin = await factory.CreateUserAsync("media.upload@example.com", "Temporary!Pass123");
        await factory.CreateAccessProfileAsync(admin.Id, StaffRole.ADMIN);
        using var client = Client(factory);
        await Login(client, admin.Email!);
        var csrf = await GetCsrf(client);

        using var createTeam = await SendJson(client, "/api/settings/teams", new
        {
            name = "Media team",
            trackingLevel = "STANDARD"
        }, csrf);
        Assert.Equal(HttpStatusCode.Created, createTeam.StatusCode);
        using var team = await JsonDocument.ParseAsync(await createTeam.Content.ReadAsStreamAsync());
        var teamId = team.RootElement.GetProperty("id").GetGuid();

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(JsonSerializer.Serialize(new
        {
            teamId,
            category = "IMAGE",
            title = "Training image"
        }), System.Text.Encoding.UTF8, "application/json"), "metadata");
        var file = new ByteArrayContent([137, 80, 78, 71]);
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        form.Add(file, "file", "training.png");
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/media/assets") { Content = form };
        request.Headers.Add("X-CSRF-TOKEN", csrf);
        using var response = await client.SendAsync(request);

        Assert.True(response.StatusCode == HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        using var created = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var mediaId = created.RootElement.GetProperty("id").GetGuid();

        using var content = await client.GetAsync($"/api/media/{mediaId}/content");
        Assert.True(content.StatusCode == HttpStatusCode.OK, $"{content.StatusCode}: {await content.Content.ReadAsStringAsync()}");
        Assert.Equal("image/png", content.Content.Headers.ContentType?.MediaType);
    }

    private static HttpClient Client(WebApplicationFactory<Program> factory) => factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
    private static async Task Login(HttpClient client, string email)
    {
        using var csrf = await client.GetAsync("/api/auth/csrf");
        using var payload = await JsonDocument.ParseAsync(await csrf.Content.ReadAsStreamAsync());
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new
            {
                email,
                password = "Temporary!Pass123"
            })
        };
        request.Headers.Add("X-CSRF-TOKEN", payload.RootElement.GetProperty("requestToken").GetString());
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<string> GetCsrf(HttpClient client)
    {
        using var response = await client.GetAsync("/api/auth/csrf");
        using var body = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return body.RootElement.GetProperty("requestToken").GetString()!;
    }

    private static async Task<HttpResponseMessage> SendJson(HttpClient client, string path, object payload, string csrf)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(payload) };
        request.Headers.Add("X-CSRF-TOKEN", csrf);
        return await client.SendAsync(request);
    }
}
