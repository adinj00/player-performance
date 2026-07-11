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
        var players = endpoints.MapGroup("/api/players").RequireAuthorization(StaffAuthorizationPolicies.AdminOnly).WithTags("Players");
        players.MapGet("", ListAsync);
        players.MapGet("/{playerId:guid}", GetAsync);
        players.MapPost("", CreateAsync);
        players.MapPatch("/{playerId:guid}", UpdateAsync);
        players.MapPost("/{playerId:guid}/activate", ActivateAsync);
        players.MapPost("/{playerId:guid}/deactivate", DeactivateAsync);
        players.MapPost("/{playerId:guid}/archive", ArchiveAsync);
        players.MapPost("/{playerId:guid}/restore", RestoreAsync);
        return endpoints;
    }

    private static async Task<Results<Ok<PagedPlayerListResponse>, ProblemHttpResult>> ListAsync(IPlayersService service, HttpContext context, CancellationToken ct, string? search = null, PlayerRecordStatus? status = null, bool includeArchived = false, int page = 1, int pageSize = 25)
    {
        var result = await service.ListAsync(new PlayerListQuery { Search = search, Status = status, IncludeArchived = includeArchived, Page = page, PageSize = pageSize }, ct);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : Problem(result.Error, context);
    }

    private static async Task<Results<Ok<PlayerSummaryResponse>, NotFound>> GetAsync(Guid playerId, IPlayersService service, CancellationToken ct) => (await service.GetAsync(playerId, ct)) is { } response ? TypedResults.Ok(response) : TypedResults.NotFound();

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
    private static IResult ToResult(Result<PlayerSummaryResponse> result, HttpContext context) => result.IsSuccess ? TypedResults.Ok(result.Value) : Problem(result.Error, context);
    private static ProblemHttpResult Problem(Error error, HttpContext context)
    {
        var status = error.Code switch
        {
            "not_found" => StatusCodes.Status404NotFound,
            "archived_record" => StatusCodes.Status409Conflict,
            "validation_failed" => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };
        return TypedResults.Problem(new ProblemDetails { Status = status, Title = "Request could not be completed", Detail = error.Message, Extensions = { ["code"] = error.Code, ["traceId"] = context.TraceIdentifier } });
    }
}
