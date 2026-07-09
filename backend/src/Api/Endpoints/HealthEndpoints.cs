using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using PlayerPerformance.Api.Configuration;

namespace PlayerPerformance.Api.Endpoints;

internal static class HealthEndpoints
{
    private static readonly JsonSerializerOptions ReadinessJsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", (IOptions<PlayerPerformanceOptions> options) =>
            TypedResults.Ok(new HealthResponse(
                "ok",
                options.Value.ServiceName,
                DateTimeOffset.UtcNow)));

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = static registration => registration.Tags.Contains("ready"),
            ResponseWriter = WriteReadinessResponseAsync
        });

        return endpoints;
    }

    private static Task WriteReadinessResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = new ReadinessHealthResponse(
            report.Status.ToString().ToLowerInvariant(),
            DateTimeOffset.UtcNow,
            report.Entries.ToDictionary(
                static entry => entry.Key,
                static entry => new ReadinessHealthCheckResponse(
                    entry.Value.Status.ToString().ToLowerInvariant())));

        return JsonSerializer.SerializeAsync(
            context.Response.Body,
            response,
            ReadinessJsonSerializerOptions);
    }
}

internal sealed record HealthResponse(
    string Status,
    string Service,
    DateTimeOffset TimestampUtc);

internal sealed record ReadinessHealthResponse(
    string Status,
    DateTimeOffset TimestampUtc,
    IReadOnlyDictionary<string, ReadinessHealthCheckResponse> Checks);

internal sealed record ReadinessHealthCheckResponse(string Status);
