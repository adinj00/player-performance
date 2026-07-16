using Microsoft.AspNetCore.Http.HttpResults;
using PlayerPerformance.Domain.Physical;
namespace PlayerPerformance.Api.Endpoints.Training;
internal static class PhysicalMetricEndpoints
{
    public static IEndpointRouteBuilder MapPhysicalMetricEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/physical-metrics/catalog", (IPhysicalMetricCatalog catalog) => TypedResults.Ok(catalog.All))
            .RequireAuthorization().WithTags("Physical metrics").WithName("GetPhysicalMetricCatalog");
        return endpoints;
    }
}
