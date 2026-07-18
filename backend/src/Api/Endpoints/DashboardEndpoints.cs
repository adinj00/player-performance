using PlayerPerformance.Application.Dashboard;

namespace PlayerPerformance.Api.Endpoints;

internal static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var dashboard = endpoints.MapGroup("/api/dashboard").RequireAuthorization().WithTags("Dashboard");
        dashboard.MapGet("/context-options", ContextOptionsAsync);
        dashboard.MapGet("/overview", OverviewAsync);
        return endpoints;
    }

    private static async Task<IResult> ContextOptionsAsync(IDashboardService service, CancellationToken ct)
        => await service.GetContextOptionsAsync(ct) is { } response ? TypedResults.Ok(response) : TypedResults.Forbid();

    private static async Task<IResult> OverviewAsync(Guid? teamId, Guid? seasonId, IDashboardService service, CancellationToken ct)
    {
        if (teamId is not { } team || seasonId is not { } season || team == Guid.Empty || season == Guid.Empty)
            return TypedResults.BadRequest();
        var result = await service.GetOverviewAsync(team, season, ct);
        return result.Response is not null ? TypedResults.Ok(result.Response) : result.Failure!.Code switch
        {
            "forbidden" => TypedResults.Forbid(),
            "not_found" => TypedResults.NotFound(),
            _ => TypedResults.BadRequest()
        };
    }
}
