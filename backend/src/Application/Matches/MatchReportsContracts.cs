using FluentValidation;
using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Matches;

namespace PlayerPerformance.Application.Matches;

public enum MatchReportAction
{
    VIEW = 1, EDIT = 2, SUBMIT_FOR_REVIEW = 3, VERIFY = 4, REQUEST_CORRECTION = 5, ARCHIVE = 6
}
public sealed record RequestCorrectionRequest(string Reason);
public sealed record MatchReportListQuery(Guid? SeasonId, Guid? TeamId, Guid? CompetitionId, MatchReportStatus? Status, DateTime? DateFrom, DateTime? DateTo, int Page = 1, int PageSize = 25);
public sealed record MatchReportActorResponse(Guid UserId, DateTime AtUtc);
public sealed record MatchReportCorrectionResponse(Guid UserId, DateTime AtUtc, string Reason);
public sealed record MatchReportResponse(Guid Id, Guid MatchId, MatchReportStatus Status, IReadOnlyList<MatchReportAction> AllowedActions, Guid CreatedByUserId, DateTime CreatedAtUtc, MatchReportActorResponse? Submitted, MatchReportActorResponse? Verified, MatchReportCorrectionResponse? LastCorrection, MatchReportActorResponse? Archived);
public sealed record MatchReportListItemResponse(Guid Id, Guid MatchId, DateTime KickoffAtUtc, MatchReferenceResponse Team, MatchReferenceResponse Opponent, MatchReferenceResponse Competition, MatchReportStatus Status, IReadOnlyList<MatchReportAction> AllowedActions, MatchReportActorResponse? Submitted, MatchReportActorResponse? Verified, MatchReportCorrectionResponse? LastCorrection);
public sealed record PagedMatchReportListResponse(IReadOnlyList<MatchReportListItemResponse> Items, int Page, int PageSize, int TotalCount, int TotalPages);
public sealed class RequestCorrectionRequestValidator : AbstractValidator<RequestCorrectionRequest>
{
    public RequestCorrectionRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().Must(x => !string.IsNullOrWhiteSpace(x)).MaximumLength(MatchReport.MaxCorrectionReasonLength);
    }
}
public sealed class MatchReportListQueryValidator : AbstractValidator<MatchReportListQuery>
{
    public MatchReportListQueryValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, 100000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x).Must(x => !x.DateFrom.HasValue || !x.DateTo.HasValue || x.DateFrom <= x.DateTo);
    }
}
public interface IMatchReportsService
{
    Task<Result<MatchReportResponse>> CreateAsync(Guid matchId, CancellationToken ct);
    Task<MatchReportResponse?> GetByMatchAsync(Guid matchId, CancellationToken ct);
    Task<Result<PagedMatchReportListResponse>> ListAsync(MatchReportListQuery query, CancellationToken ct);
    Task<Result<MatchReportResponse>> SubmitAsync(Guid reportId, CancellationToken ct);
    Task<Result<MatchReportResponse>> VerifyAsync(Guid reportId, CancellationToken ct);
    Task<Result<MatchReportResponse>> RequestCorrectionAsync(Guid reportId, RequestCorrectionRequest request, CancellationToken ct);
    Task<Result<MatchReportResponse>> ArchiveAsync(Guid reportId, CancellationToken ct);
}
public interface IMatchReportWorkflowGuard
{
    Task<Result> EnsureEditableAsync(Guid matchId, CancellationToken ct);
    Task<Result> EnsureMatchCanBeArchivedAsync(Guid matchId, CancellationToken ct);
}
public interface IMatchReportsRepository
{
    Task<MatchReportAggregate?> GetAggregateAsync(Guid reportId, CancellationToken ct);
    Task<MatchReportAggregate?> GetByMatchAggregateAsync(Guid matchId, CancellationToken ct);
    Task<MatchReportReadModel?> GetReadByMatchAsync(Guid matchId, IReadOnlyCollection<Guid>? scope, CancellationToken ct);
    Task<PagedMatchReportReadModel> ListAsync(MatchReportListQuery query, IReadOnlyCollection<Guid>? scope, IReadOnlyCollection<MatchReportStatus> visibleStatuses, CancellationToken ct);
    Task<bool> ExistsForMatchAsync(Guid matchId, CancellationToken ct);
    void Add(MatchReport report); Task SaveChangesAsync(CancellationToken ct);
}
public sealed record MatchReportAggregate(MatchReport Report, Match Match, int AppearanceCount);
public sealed record MatchReportReadModel(MatchReport Report, Match Match, string TeamName, string OpponentName, string CompetitionName);
public sealed record PagedMatchReportReadModel(IReadOnlyList<MatchReportReadModel> Items, int TotalCount);
internal static class MatchReportErrors
{
    public static readonly Error NotFound = new("not_found", "The requested match report was not found.");
    public static readonly Error Forbidden = new("forbidden", "You do not have access to this match report.");
    public static readonly Error Validation = new("validation_failed", "One or more fields are invalid.");
    public static readonly Error Conflict = new("report_conflict", "The requested report workflow change is not allowed.");
    public static readonly Error Duplicate = new("duplicate_report", "A report already exists for this match.");
    public static readonly Error WorkflowLocked = new("report_workflow_locked", "Match data is locked while the report is under review, verified, or archived.");
}
