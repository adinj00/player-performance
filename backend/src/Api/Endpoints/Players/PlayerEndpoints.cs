using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PlayerPerformance.Api.Authentication;
using PlayerPerformance.Api.Authorization;
using PlayerPerformance.Application.Players;
using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Players;

namespace PlayerPerformance.Api.Endpoints.Players;

internal static class PlayerEndpoints
{
    public static IEndpointRouteBuilder MapPlayerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var players = endpoints.MapGroup("/api/players").RequireAuthorization().WithTags("Players");
        players.MapGet("", ListAsync);
        players.MapGet("/{playerId:guid}", GetAsync);
        players.MapGet("/{playerId:guid}/assignments", ListAssignmentsAsync);
        players.MapPost("", CreateAsync).RequireAuthorization(StaffAuthorizationPolicies.AdminOnly);
        players.MapPatch("/{playerId:guid}", UpdateAsync).RequireAuthorization(StaffAuthorizationPolicies.AdminOnly);
        players.MapPost("/{playerId:guid}/activate", ActivateAsync).RequireAuthorization(StaffAuthorizationPolicies.AdminOnly);
        players.MapPost("/{playerId:guid}/deactivate", DeactivateAsync).RequireAuthorization(StaffAuthorizationPolicies.AdminOnly);
        players.MapPost("/{playerId:guid}/archive", ArchiveAsync).RequireAuthorization(StaffAuthorizationPolicies.AdminOnly);
        players.MapPost("/{playerId:guid}/restore", RestoreAsync).RequireAuthorization(StaffAuthorizationPolicies.AdminOnly);
        players.MapPost("/{playerId:guid}/assignments", CreateAssignmentAsync).RequireAuthorization(StaffAuthorizationPolicies.AdminOnly);
        players.MapPost("/{playerId:guid}/assignments/{assignmentId:guid}/end", EndAssignmentAsync).RequireAuthorization(StaffAuthorizationPolicies.AdminOnly);
        return endpoints;
    }

    private static async Task<Results<Ok<PagedPlayerListResponse>, ProblemHttpResult>> ListAsync(IPlayersService service, HttpContext context, CancellationToken ct, string? search = null, PlayerRecordStatus? status = null, bool includeArchived = false, int page = 1, int pageSize = 25, Guid? teamId = null)
    {
        var result = await service.ListAsync(new PlayerListQuery { Search = search, Status = status, IncludeArchived = includeArchived, Page = page, PageSize = pageSize, TeamId = teamId }, ct);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : Problem(result.Error, context);
    }

    private static async Task<Results<Ok<PlayerSummaryResponse>, NotFound>> GetAsync(Guid playerId, IPlayersService service, CancellationToken ct) => (await service.GetAsync(playerId, ct)) is { } response ? TypedResults.Ok(response) : TypedResults.NotFound();
    private static async Task<Results<Ok<IReadOnlyList<PlayerTeamAssignmentResponse>>, NotFound>> ListAssignmentsAsync(Guid playerId, IPlayerTeamAssignmentsService service, CancellationToken ct) => (await service.ListAsync(playerId, ct)) is { } assignments ? TypedResults.Ok(assignments) : TypedResults.NotFound();

    private static async Task<IResult> CreateAsync(HttpContext context, IAntiforgery antiforgery, CreatePlayerRequest request, IPlayersService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToCreated(await service.CreateAsync(request, ct), context);
    }

    private static async Task<IResult> UpdateAsync(Guid playerId, HttpContext context, IAntiforgery antiforgery, UpdatePlayerRequest request, IPlayersService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToResult(await service.UpdateAsync(playerId, request, ct), context);
    }
    private static async Task<IResult> CreateAssignmentAsync(Guid playerId, HttpContext context, IAntiforgery antiforgery, CreatePlayerTeamAssignmentRequest request, IPlayerTeamAssignmentsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToCreatedAssignment(await service.CreateAsync(playerId, request, ct), context);
    }
    private static async Task<IResult> EndAssignmentAsync(Guid playerId, Guid assignmentId, HttpContext context, IAntiforgery antiforgery, EndPlayerTeamAssignmentRequest request, IPlayerTeamAssignmentsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToResult(await service.EndAsync(playerId, assignmentId, request, ct), context);
    }

    private static async Task<IResult> ActivateAsync(Guid playerId, HttpContext context, IAntiforgery antiforgery, IPlayersService service, CancellationToken ct) => await ChangeStateAsync(playerId, context, antiforgery, service.ActivateAsync, ct);
    private static async Task<IResult> DeactivateAsync(Guid playerId, HttpContext context, IAntiforgery antiforgery, IPlayersService service, CancellationToken ct) => await ChangeStateAsync(playerId, context, antiforgery, service.DeactivateAsync, ct);
    private static async Task<IResult> ArchiveAsync(Guid playerId, HttpContext context, IAntiforgery antiforgery, IPlayersService service, CancellationToken ct) => await ChangeStateAsync(playerId, context, antiforgery, service.ArchiveAsync, ct);
    private static async Task<IResult> RestoreAsync(Guid playerId, HttpContext context, IAntiforgery antiforgery, IPlayersService service, CancellationToken ct) => await ChangeStateAsync(playerId, context, antiforgery, service.RestoreAsync, ct);

    private static async Task<IResult> ChangeStateAsync(Guid playerId, HttpContext context, IAntiforgery antiforgery, Func<Guid, CancellationToken, Task<Result<PlayerSummaryResponse>>> transition, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToResult(await transition(playerId, ct), context);
    }

    private static IResult ToCreated(Result<PlayerSummaryResponse> result, HttpContext context) => result.IsSuccess ? TypedResults.Created($"/api/players/{result.Value.Id}", result.Value) : Problem(result.Error, context);
    private static IResult ToCreatedAssignment(Result<PlayerTeamAssignmentResponse> result, HttpContext context) => result.IsSuccess ? TypedResults.Created($"/api/players/{result.Value.PlayerId}/assignments/{result.Value.Id}", result.Value) : Problem(result.Error, context);
    private static IResult ToResult(Result<PlayerSummaryResponse> result, HttpContext context) => result.IsSuccess ? TypedResults.Ok(result.Value) : Problem(result.Error, context);
    private static IResult ToResult(Result<PlayerTeamAssignmentResponse> result, HttpContext context) => result.IsSuccess ? TypedResults.Ok(result.Value) : Problem(result.Error, context);
    private static ProblemHttpResult Problem(Error error, HttpContext context)
    {
        var status = error.Code switch
        {
            "not_found" => StatusCodes.Status404NotFound,
            "archived_record" => StatusCodes.Status409Conflict,
            "player_assignment_overlap" or "player_assignment_already_ended" or "player_has_current_assignments" or "player_not_active" or "team_not_active" => StatusCodes.Status409Conflict,
            "forbidden" => StatusCodes.Status403Forbidden,
            "validation_failed" => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };
        return TypedResults.Problem(new ProblemDetails { Status = status, Title = "Request could not be completed", Detail = error.Message, Extensions = { ["code"] = error.Code, ["traceId"] = context.TraceIdentifier } });
    }
}
