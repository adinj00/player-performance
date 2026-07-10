namespace PlayerPerformance.Api.Endpoints;

internal static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder endpoints, IHostEnvironment environment)
    {
        endpoints.MapHealthEndpoints();
        endpoints.MapAuthEndpoints();

        if (environment.IsEnvironment("Testing"))
        {
            endpoints.MapGet("/_test/protected", () => TypedResults.NoContent())
                .RequireAuthorization();
        }

        return endpoints;
    }
}
