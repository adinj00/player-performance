using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PlayerPerformance.IntegrationTests.Api;

public sealed class AuthEndpointsTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task GetSession_ShouldReturnExplicitUnauthenticatedPayload_WhenCallerIsUnauthenticated()
    {
        using var response = await _client.GetAsync("/api/auth/session");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        Assert.False(payload.RootElement.GetProperty("isAuthenticated").GetBoolean());
        Assert.Equal(JsonValueKind.Null, payload.RootElement.GetProperty("user").ValueKind);
    }

    [Fact]
    public async Task GetCsrf_ShouldReturnHeaderMetadataAndSetReadableCookie()
    {
        using var response = await _client.GetAsync("/api/auth/csrf");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.Equal("X-CSRF-TOKEN", payload.RootElement.GetProperty("csrfTokenHeaderName").GetString());

        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var setCookieValues));

        var csrfCookie = Assert.Single(
            setCookieValues,
            value => value.StartsWith("XSRF-TOKEN=", StringComparison.Ordinal));
        Assert.Contains("path=/", csrfCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", csrfCookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("httponly", csrfCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", csrfCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PostLogout_ShouldReturnUnauthorizedProblemDetails_WhenCallerIsUnauthenticated()
    {
        using var response = await _client.PostAsync("/api/auth/logout", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.False(response.Headers.Contains("Location"));

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Equal((int)HttpStatusCode.Unauthorized, problemDetails!.Status);
        Assert.Equal("Unauthorized", problemDetails.Title);
    }
}
