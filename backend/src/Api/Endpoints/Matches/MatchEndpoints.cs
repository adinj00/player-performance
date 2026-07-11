using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PlayerPerformance.Api.Authentication;
using PlayerPerformance.Api.Authorization;
using PlayerPerformance.Application.Matches;
using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Matches;

namespace PlayerPerformance.Api.Endpoints.Matches;

internal static class MatchEndpoints
{
    public static IEndpointRouteBuilder MapMatchEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var matches = endpoints.MapGroup("/api/matches").RequireAuthorization().WithTags("Matches");
        matches.MapGet("", ListAsync);
        matches.MapGet("/{matchId:guid}", GetAsync);
        matches.MapGet("/{matchId:guid}/lineup", GetLineupAsync);
        matches.MapPost("", CreateAsync);
        matches.MapPatch("/{matchId:guid}", UpdateAsync);
        matches.MapPut("/{matchId:guid}/lineup", SaveLineupAsync);
        matches.MapPost("/{matchId:guid}/archive", ArchiveAsync).RequireAuthorization(StaffAuthorizationPolicies.AdminOnly);
        matches.MapPost("/{matchId:guid}/restore", RestoreAsync).RequireAuthorization(StaffAuthorizationPolicies.AdminOnly);
        return endpoints;
    }
    private static async Task<Results<Ok<PagedMatchListResponse>, ProblemHttpResult>> ListAsync(IMatchesService service, HttpContext context, CancellationToken ct, Guid? seasonId = null, Guid? teamId = null, Guid? competitionId = null, Guid? opponentId = null, MatchStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null, bool includeArchived = false, int page = 1, int pageSize = 25)
    {
        var result = await service.ListAsync(new(seasonId, teamId, competitionId, opponentId, status, dateFrom, dateTo, includeArchived, page, pageSize), ct);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : Problem(result.Error, context);
    }
    private static async Task<Results<Ok<MatchResponse>, NotFound>> GetAsync(Guid matchId, IMatchesService service, CancellationToken ct) => (await service.GetAsync(matchId, ct)) is { } response ? TypedResults.Ok(response) : TypedResults.NotFound();
    private static async Task<Results<Ok<MatchLineupResponse>, NotFound>> GetLineupAsync(Guid matchId, IMatchesService service, CancellationToken ct) => (await service.GetLineupAsync(matchId, ct)) is { } response ? TypedResults.Ok(response) : TypedResults.NotFound();
    private static async Task<IResult> CreateAsync(HttpContext context, IAntiforgery antiforgery, CreateMatchRequest request, IMatchesService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToCreated(await service.CreateAsync(request, ct), context);
    }
    private static async Task<IResult> UpdateAsync(Guid matchId, HttpContext context, IAntiforgery antiforgery, UpdateMatchRequest request, IMatchesService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToResult(await service.UpdateAsync(matchId, request, ct), context);
    }
    private static async Task<IResult> SaveLineupAsync(Guid matchId, HttpContext context, IAntiforgery antiforgery, SaveMatchLineupRequest request, IMatchesService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToLineupResult(await service.SaveLineupAsync(matchId, request, ct), context);
    }
    private static async Task<IResult> ArchiveAsync(Guid matchId, HttpContext context, IAntiforgery antiforgery, IMatchesService service, CancellationToken ct) => await ChangeArchiveStateAsync(matchId, context, antiforgery, service.ArchiveAsync, ct);
    private static async Task<IResult> RestoreAsync(Guid matchId, HttpContext context, IAntiforgery antiforgery, IMatchesService service, CancellationToken ct) => await ChangeArchiveStateAsync(matchId, context, antiforgery, service.RestoreAsync, ct);
    private static async Task<IResult> ChangeArchiveStateAsync(Guid matchId, HttpContext context, IAntiforgery antiforgery, Func<Guid, CancellationToken, Task<Result<MatchResponse>>> operation, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToResult(await operation(matchId, ct), context);
    }
    private static IResult ToCreated(Result<MatchResponse> result, HttpContext context) => result.IsSuccess ? TypedResults.Created($"/api/matches/{result.Value.Id}", result.Value) : Problem(result.Error, context);
    private static IResult ToResult(Result<MatchResponse> result, HttpContext context) => result.IsSuccess ? TypedResults.Ok(result.Value) : Problem(result.Error, context);
    private static IResult ToLineupResult(Result<MatchLineupResponse> result, HttpContext context) => result.IsSuccess ? TypedResults.Ok(result.Value) : Problem(result.Error, context);
    private static ProblemHttpResult Problem(Error error, HttpContext context)
    {
        var status = error.Code switch { "not_found" => StatusCodes.Status404NotFound, "forbidden" => StatusCodes.Status403Forbidden, "duplicate_match" or "match_conflict" => StatusCodes.Status409Conflict, "validation_failed" or "invalid_references" => StatusCodes.Status422UnprocessableEntity, _ => StatusCodes.Status400BadRequest };
        return TypedResults.Problem(new ProblemDetails { Status = status, Title = "Request could not be completed", Detail = error.Message, Extensions = { ["code"] = error.Code, ["traceId"] = context.TraceIdentifier } });
    }
}
