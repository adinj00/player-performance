using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PlayerPerformance.Domain.Users;

namespace PlayerPerformance.IntegrationTests.Api;

public sealed class AuthEndpointsTests : IClassFixture<IdentityTestApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(IdentityTestApplicationFactory factory)
    {
        _factory = factory;
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
        Assert.False(string.IsNullOrWhiteSpace(payload.RootElement.GetProperty("requestToken").GetString()));

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

    [Fact]
    public async Task PostLogin_ShouldIssueCookieAndReturnSafeRequiredPasswordChangeSession()
    {
        await _factory.CreateUserAsync("first.admin@example.com", "Temporary!Pass123", requiresPasswordChange: true);
        var csrfToken = await GetCsrfTokenAsync();

        using var response = await PostJsonAsync("/api/auth/login", csrfToken, new
        {
            email = "first.admin@example.com",
            password = "Temporary!Pass123"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.Contains(cookies, value => value.StartsWith("player-performance.auth=", StringComparison.Ordinal));

        using var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        Assert.True(payload.RootElement.GetProperty("isAuthenticated").GetBoolean());
        Assert.True(payload.RootElement.GetProperty("user").GetProperty("mustChangePassword").GetBoolean());
        Assert.Equal("ACTIVE", payload.RootElement.GetProperty("user").GetProperty("accountStatus").GetString());
    }

    [Fact]
    public async Task PostLogin_ShouldReturnGenericUnauthorizedProblem_WhenCredentialsAreInvalid()
    {
        var csrfToken = await GetCsrfTokenAsync();

        using var response = await PostJsonAsync("/api/auth/login", csrfToken, new
        {
            email = "missing@example.com",
            password = "Wrong!Password123"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Invalid email or password.", problemDetails!.Detail);
        Assert.Equal("invalid_credentials", problemDetails.Extensions["code"]?.ToString());
    }

    [Theory]
    [InlineData(UserAccountStatus.DISABLED)]
    [InlineData(UserAccountStatus.LOCKED)]
    public async Task PostLogin_ShouldRejectUnavailableAccounts(UserAccountStatus accountStatus)
    {
        var email = $"{accountStatus.ToString().ToLowerInvariant()}@example.com";
        await _factory.CreateUserAsync(email, "Temporary!Pass123", accountStatus);
        var csrfToken = await GetCsrfTokenAsync();

        using var response = await PostJsonAsync("/api/auth/login", csrfToken, new
        {
            email,
            password = "Temporary!Pass123"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.False(response.Headers.TryGetValues("Set-Cookie", out var cookies)
            && cookies.Any(value => value.StartsWith("player-performance.auth=", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task RequiredPasswordChange_ShouldGateProtectedRouteUntilPasswordIsChanged()
    {
        const string email = "required-change@example.com";
        await _factory.CreateUserAsync(email, "Temporary!Pass123", requiresPasswordChange: true);
        var csrfToken = await GetCsrfTokenAsync();
        using var login = await PostJsonAsync("/api/auth/login", csrfToken, new { email, password = "Temporary!Pass123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        using var blocked = await _client.GetAsync("/_test/protected");
        Assert.Equal(HttpStatusCode.Forbidden, blocked.StatusCode);
        var blockedProblem = await blocked.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("password_change_required", blockedProblem!.Extensions["code"]?.ToString());

        using var session = await _client.GetAsync("/api/auth/session");
        Assert.Equal(HttpStatusCode.OK, session.StatusCode);

        csrfToken = await GetCsrfTokenAsync();
        using var change = await PostJsonAsync("/api/auth/change-password", csrfToken, new
        {
            currentPassword = "Temporary!Pass123",
            newPassword = "Replacement!Pass123",
            confirmPassword = "Replacement!Pass123"
        });

        Assert.Equal(HttpStatusCode.OK, change.StatusCode);
        using var changePayload = await JsonDocument.ParseAsync(await change.Content.ReadAsStreamAsync());
        Assert.False(changePayload.RootElement.GetProperty("user").GetProperty("mustChangePassword").GetBoolean());

        using var allowed = await _client.GetAsync("/_test/protected");
        Assert.Equal(HttpStatusCode.NoContent, allowed.StatusCode);

        csrfToken = await GetCsrfTokenAsync();
        using var logout = await PostJsonAsync("/api/auth/logout", csrfToken, new { });
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        csrfToken = await GetCsrfTokenAsync();
        using var oldPasswordLogin = await PostJsonAsync("/api/auth/login", csrfToken, new
        {
            email,
            password = "Temporary!Pass123"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, oldPasswordLogin.StatusCode);

        csrfToken = await GetCsrfTokenAsync();
        using var newPasswordLogin = await PostJsonAsync("/api/auth/login", csrfToken, new
        {
            email,
            password = "Replacement!Pass123"
        });
        Assert.Equal(HttpStatusCode.OK, newPasswordLogin.StatusCode);
    }

    [Fact]
    public async Task FailedPasswordChange_ShouldNotClearRequiredPasswordChangeFlag()
    {
        const string email = "failed-change@example.com";
        await _factory.CreateUserAsync(email, "Temporary!Pass123", requiresPasswordChange: true);
        var csrfToken = await GetCsrfTokenAsync();
        using var login = await PostJsonAsync("/api/auth/login", csrfToken, new { email, password = "Temporary!Pass123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        csrfToken = await GetCsrfTokenAsync();
        using var change = await PostJsonAsync("/api/auth/change-password", csrfToken, new
        {
            currentPassword = "incorrect-password",
            newPassword = "Replacement!Pass123",
            confirmPassword = "Replacement!Pass123"
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, change.StatusCode);

        csrfToken = await GetCsrfTokenAsync();
        using var policyFailure = await PostJsonAsync("/api/auth/change-password", csrfToken, new
        {
            currentPassword = "Temporary!Pass123",
            newPassword = "weak",
            confirmPassword = "weak"
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, policyFailure.StatusCode);

        using var session = await _client.GetAsync("/api/auth/session");
        using var payload = await JsonDocument.ParseAsync(await session.Content.ReadAsStreamAsync());
        Assert.True(payload.RootElement.GetProperty("user").GetProperty("mustChangePassword").GetBoolean());
    }

    private readonly TestApplicationFactory _factory;

    private async Task<string> GetCsrfTokenAsync()
    {
        using var response = await _client.GetAsync("/api/auth/csrf");
        using var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return payload.RootElement.GetProperty("requestToken").GetString()!;
    }

    private Task<HttpResponseMessage> PostJsonAsync(string path, string csrfToken, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Add("X-CSRF-TOKEN", csrfToken);
        return _client.SendAsync(request);
    }
}
