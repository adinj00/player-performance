namespace PlayerPerformance.IntegrationTests.Configuration;

public sealed class CorsConfigurationTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;

    public CorsConfigurationTests(TestApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PreflightRequest_ShouldAllowConfiguredFrontendOrigin()
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/auth/session");
        request.Headers.Add("Origin", "https://frontend.test");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(
            "https://frontend.test",
            response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.Equal(
            "true",
            response.Headers.GetValues("Access-Control-Allow-Credentials").Single());
    }
}
