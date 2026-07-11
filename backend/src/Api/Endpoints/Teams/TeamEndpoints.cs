using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using PlayerPerformance.Api.Authentication;
using PlayerPerformance.Api.Authorization;
using PlayerPerformance.Application.Teams;
using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;

namespace PlayerPerformance.Api.Endpoints.Teams;

internal static class TeamEndpoints
{
    public static IEndpointRouteBuilder MapTeamEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var teams = endpoints.MapGroup("/api/settings/teams").RequireAuthorization(StaffAuthorizationPolicies.AdminOnly).WithTags("Settings");
        teams.MapGet("", (ITeamsService service, CancellationToken ct, bool includeArchived = false) => service.ListAsync(includeArchived, ct));
        teams.MapGet("/{teamId:guid}", GetAsync);
        teams.MapPost("", CreateAsync);
        teams.MapPatch("/{teamId:guid}", UpdateAsync);
        teams.MapPut("/order", ReorderAsync);
        teams.MapPost("/{teamId:guid}/activate", ActivateAsync);
        teams.MapPost("/{teamId:guid}/deactivate", DeactivateAsync);
        teams.MapPost("/{teamId:guid}/archive", ArchiveAsync);
        teams.MapPost("/{teamId:guid}/restore", RestoreAsync);
        return endpoints;
    }

    private static async Task<IResult> GetAsync(Guid teamId, ITeamsService service, CancellationToken ct) => (await service.GetAsync(teamId, ct)) is { } response ? TypedResults.Ok(response) : TypedResults.NotFound();
    private static async Task<IResult> CreateAsync(HttpContext context, IAntiforgery antiforgery, CreateTeamRequest request, ITeamsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        if (failure is not null)
            return failure;
        var result = await service.CreateAsync(request, ct);
        return result.IsSuccess ? TypedResults.Created($"/api/settings/teams/{result.Value.Id}", result.Value) : ToProblem(result.Error, context);
    }
    private static async Task<IResult> UpdateAsync(Guid teamId, HttpContext context, IAntiforgery antiforgery, UpdateTeamRequest request, ITeamsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToResult(await service.UpdateAsync(teamId, request, ct), context);
    }
    private static async Task<IResult> ReorderAsync(HttpContext context, IAntiforgery antiforgery, ReorderTeamsRequest request, ITeamsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToResult(await service.ReorderAsync(request, ct), context);
    }
    private static async Task<IResult> ActivateAsync(Guid teamId, HttpContext context, IAntiforgery antiforgery, ITeamsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToResult(await service.ActivateAsync(teamId, ct), context);
    }
    private static async Task<IResult> DeactivateAsync(Guid teamId, HttpContext context, IAntiforgery antiforgery, ITeamsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToResult(await service.DeactivateAsync(teamId, ct), context);
    }
    private static async Task<IResult> ArchiveAsync(Guid teamId, HttpContext context, IAntiforgery antiforgery, ITeamsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToResult(await service.ArchiveAsync(teamId, ct), context);
    }
    private static async Task<IResult> RestoreAsync(Guid teamId, HttpContext context, IAntiforgery antiforgery, ITeamsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToResult(await service.RestoreAsync(teamId, ct), context);
    }
    private static IResult ToResult<T>(Result<T> result, HttpContext context) => result.IsSuccess ? TypedResults.Ok(result.Value) : ToProblem(result.Error, context);
    private static IResult ToProblem(Error error, HttpContext context)
    {
        var status = error.Code switch
        {
            "not_found" => StatusCodes.Status404NotFound,
            "duplicate_name" or "archived_record" or "invalid_team_order" => StatusCodes.Status409Conflict,
            "validation_failed" => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };
        return TypedResults.Problem(new ProblemDetails { Status = status, Title = "Request could not be completed", Detail = error.Message, Extensions = { ["code"] = error.Code, ["traceId"] = context.TraceIdentifier } });
    }
}
