using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using PlayerPerformance.Api.Authentication;
using PlayerPerformance.Application.Matches;
using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Application.Auditing;
using PlayerPerformance.Domain.Auditing;

namespace PlayerPerformance.Api.Endpoints.Matches;

internal static class MatchReportEndpoints
{
    public static IEndpointRouteBuilder MapMatchReportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var matches = endpoints.MapGroup("/api/matches").RequireAuthorization().WithTags("Match reports");
        matches.MapGet("/{matchId:guid}/report", GetAsync);
        matches.MapPost("/{matchId:guid}/report", CreateAsync);
        var reports = endpoints.MapGroup("/api/match-reports").RequireAuthorization().WithTags("Match reports");
        reports.MapGet("", ListAsync);
        reports.MapGet("/{reportId:guid}/statistics", GetStatisticsAsync);
        reports.MapGet("/{reportId:guid}/audit", GetAuditAsync);
        reports.MapPut("/{reportId:guid}/statistics", SaveStatisticsAsync);
        reports.MapPost("/{reportId:guid}/submit", SubmitAsync);
        reports.MapPost("/{reportId:guid}/verify", VerifyAsync);
        reports.MapPost("/{reportId:guid}/request-correction", RequestCorrectionAsync);
        reports.MapPost("/{reportId:guid}/archive", ArchiveAsync);
        return endpoints;
    }
    private static async Task<IResult> GetAsync(Guid matchId, IMatchReportsService service, CancellationToken ct) => (await service.GetByMatchAsync(matchId, ct)) is { } response ? TypedResults.Ok(response) : TypedResults.NotFound();
    private static async Task<IResult> ListAsync(IMatchReportsService service, HttpContext context, CancellationToken ct, Guid? seasonId = null, Guid? teamId = null, Guid? competitionId = null, MatchReportStatus? status = null, DateTime? dateFrom = null, DateTime? dateTo = null, int page = 1, int pageSize = 25) => ToResult(await service.ListAsync(new(seasonId, teamId, competitionId, status, dateFrom, dateTo, page, pageSize), ct), context);
    private static async Task<IResult> CreateAsync(Guid matchId, HttpContext context, IAntiforgery antiforgery, IMatchReportsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToCreated(await service.CreateAsync(matchId, ct), context);
    }
    private static async Task<IResult> SubmitAsync(Guid reportId, HttpContext context, IAntiforgery antiforgery, IMatchReportsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToResult(await service.SubmitAsync(reportId, ct), context);
    }
    private static async Task<IResult> GetStatisticsAsync(Guid reportId, IMatchStatisticsService service, CancellationToken ct) => (await service.GetAsync(reportId, ct)) is { } response ? TypedResults.Ok(response) : TypedResults.NotFound();
    private static async Task<IResult> GetAuditAsync(Guid reportId, IMatchStatisticsService statistics, IAuditHistoryRepository audits, int page = 1, int pageSize = 25, string? action = null, DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default)
    {
        if (await statistics.GetAsync(reportId, ct) is null)
            return TypedResults.NotFound();
        var query = new AuditHistoryQuery(page, pageSize, action, dateFrom, dateTo);
        return !AuditHistoryValidation.IsValid(query) ? TypedResults.BadRequest() : TypedResults.Ok(await audits.ListAsync(AuditEntityTypes.MatchReport, reportId, query, ct));
    }
    private static async Task<IResult> SaveStatisticsAsync(Guid reportId, HttpContext context, IAntiforgery antiforgery, SaveMatchReportStatisticsRequest request, IMatchStatisticsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToStatisticsResult(await service.SaveAsync(reportId, request, ct), context);
    }
    private static async Task<IResult> VerifyAsync(Guid reportId, HttpContext context, IAntiforgery antiforgery, IMatchReportsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToResult(await service.VerifyAsync(reportId, ct), context);
    }
    private static async Task<IResult> RequestCorrectionAsync(Guid reportId, HttpContext context, IAntiforgery antiforgery, RequestCorrectionRequest request, IMatchReportsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToResult(await service.RequestCorrectionAsync(reportId, request, ct), context);
    }
    private static async Task<IResult> ArchiveAsync(Guid reportId, HttpContext context, IAntiforgery antiforgery, IMatchReportsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        return failure ?? ToResult(await service.ArchiveAsync(reportId, ct), context);
    }
    private static IResult ToCreated(Result<MatchReportResponse> result, HttpContext context) => result.IsSuccess ? TypedResults.Created($"/api/matches/{result.Value.MatchId}/report", result.Value) : Problem(result.Error, context);
    private static IResult ToResult<T>(Result<T> result, HttpContext context) => result.IsSuccess ? TypedResults.Ok(result.Value) : Problem(result.Error, context);
    private static IResult ToStatisticsResult(Result<MatchReportStatisticsResponse> result, HttpContext context) => result.IsSuccess ? TypedResults.Ok(result.Value) : Problem(result.Error, context);
    private static IResult Problem(Error error, HttpContext context)
    {
        var status = error.Code switch
        {
            "not_found" => 404,
            "forbidden" => 403,
            "report_conflict" or "duplicate_report" or "report_workflow_locked" => 409,
            "validation_failed" or "statistics_validation_failed" => 422,
            _ => 400
        };
        return TypedResults.Problem(new ProblemDetails
        {
            Status = status,
            Title = "Request could not be completed",
            Detail = error.Message,
            Extensions = { ["code"] = error.Code, ["traceId"] = context.TraceIdentifier }
        });
    }
}
