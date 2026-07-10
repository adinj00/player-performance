using System.Text.Json;

namespace PlayerPerformance.IntegrationTests.Api;

public sealed class HealthEndpointsTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointsTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnSuccessfulSafeResponse_WhenRequestIsValid()
    {
        using var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>();

        Assert.NotNull(payload);
        Assert.Equal("ok", payload.RootElement.GetProperty("status").GetString());
        Assert.Equal("PlayerPerformance.Api.Tests", payload.RootElement.GetProperty("service").GetString());
        Assert.True(payload.RootElement.TryGetProperty("timestampUtc", out _));
        Assert.False(payload.RootElement.TryGetProperty("frontendOrigin", out _));
        Assert.False(payload.RootElement.TryGetProperty("connectionString", out _));
    }

    [Fact]
    public async Task UnknownRoute_ShouldReturnProblemDetailsNotFoundResponse_WhenEndpointDoesNotExist()
    {
        using var response = await _client.GetAsync("/missing-route");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Equal((int) HttpStatusCode.NotFound, problemDetails.Status);
        Assert.Equal("Resource not found", problemDetails.Title);
        Assert.NotNull(problemDetails.Extensions);
        Assert.True(problemDetails.Extensions.ContainsKey("traceId"));
    }

    [Fact]
    public async Task GetReadiness_ShouldReturnSafeUnhealthyResponse_WhenDatabaseIsUnavailable()
    {
        using var response = await _client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>();

        Assert.NotNull(payload);
        Assert.Equal("unhealthy", payload.RootElement.GetProperty("status").GetString());
        Assert.True(payload.RootElement.TryGetProperty("timestampUtc", out _));
        Assert.True(payload.RootElement.TryGetProperty("checks", out var checks));
        Assert.Equal("unhealthy", checks.GetProperty("database").GetProperty("status").GetString());
        Assert.False(payload.RootElement.TryGetProperty("connectionString", out _));
        Assert.False(payload.RootElement.TryGetProperty("exception", out _));
        Assert.False(payload.RootElement.TryGetProperty("stackTrace", out _));
    }
}
