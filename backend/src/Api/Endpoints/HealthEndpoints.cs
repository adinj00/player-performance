using Microsoft.Extensions.Options;
using PlayerPerformance.Api.Configuration;

namespace PlayerPerformance.Api.Endpoints;

internal static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", (IOptions<PlayerPerformanceOptions> options) =>
            TypedResults.Ok(new HealthResponse(
                "ok",
                options.Value.ServiceName,
                DateTimeOffset.UtcNow)));

        return endpoints;
    }
}

internal sealed record HealthResponse(
    string Status,
    string Service,
    DateTimeOffset TimestampUtc);
