using FluentValidation;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Application.Authorization;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Domain.Staff;

namespace PlayerPerformance.Application.Matches;

internal sealed class MatchReportsService(IMatchReportsRepository repository, IMatchesRepository matchesRepository, ICurrentUserAccess currentUserAccess, ISystemClock clock, IValidator<RequestCorrectionRequest> correctionValidator, IValidator<MatchReportListQuery> listValidator) : IMatchReportsService, IMatchReportWorkflowGuard
{
    public async Task<Result<MatchReportResponse>> CreateAsync(Guid matchId, CancellationToken ct)
    {
        var access = await currentUserAccess.GetAsync(ct);
        var match = await matchesRepository.GetAsync(matchId, ct);
        if (match is null || !CanAccess(access, match.TeamId))
            return Result<MatchReportResponse>.Failure(MatchReportErrors.NotFound);
        if (!CanEdit(access))
            return Result<MatchReportResponse>.Failure(MatchReportErrors.Forbidden);
        if (match.IsArchived || match.Status != MatchStatus.PLAYED)
            return Result<MatchReportResponse>.Failure(MatchReportErrors.Conflict);
        if (await repository.ExistsForMatchAsync(matchId, ct))
            return Result<MatchReportResponse>.Failure(MatchReportErrors.Duplicate);
        var report = MatchReport.Create(Guid.NewGuid(), matchId, access.UserId!.Value, clock.UtcNow);
        repository.Add(report);
        await repository.SaveChangesAsync(ct);
        var read = await repository.GetReadByMatchAsync(matchId, null, ct);
        return Result<MatchReportResponse>.Success(ToResponse(read!, access));
    }

    public async Task<MatchReportResponse?> GetByMatchAsync(Guid matchId, CancellationToken ct)
    {
        var access = await currentUserAccess.GetAsync(ct);
        if (!CanRead(access))
            return null;
        var read = await repository.GetReadByMatchAsync(matchId, Scope(access), ct);
        return read is null || !CanViewStatus(access, read.Report.Status) ? null : ToResponse(read, access);
    }

    public async Task<Result<PagedMatchReportListResponse>> ListAsync(MatchReportListQuery query, CancellationToken ct)
    {
        if (!(await listValidator.ValidateAsync(query, ct)).IsValid)
            return Result<PagedMatchReportListResponse>.Failure(MatchReportErrors.Validation);
        var access = await currentUserAccess.GetAsync(ct);
        if (!CanRead(access))
            return Result<PagedMatchReportListResponse>.Failure(MatchReportErrors.Forbidden);
        if (query.TeamId is { } teamId && !CanAccess(access, teamId))
            return Result<PagedMatchReportListResponse>.Failure(MatchReportErrors.Forbidden);
        var page = await repository.ListAsync(query, Scope(access), VisibleStatuses(access), ct);
        return Result<PagedMatchReportListResponse>.Success(new(page.Items.Select(x => ToListItem(x, access)).ToArray(), query.Page, query.PageSize, page.TotalCount, page.TotalCount == 0 ? 0 : (int)Math.Ceiling(page.TotalCount / (double)query.PageSize)));
    }

    public Task<Result<MatchReportResponse>> SubmitAsync(Guid reportId, CancellationToken ct) => TransitionAsync(reportId, CanEdit, (r, actor, now, aggregate) => { if (aggregate.AppearanceCount == 0) throw new ArgumentException(); r.Submit(actor, now); }, ct);
    public Task<Result<MatchReportResponse>> VerifyAsync(Guid reportId, CancellationToken ct) => TransitionAsync(reportId, CanVerify, static (r, actor, now, _) => r.Verify(actor, now), ct);
    public async Task<Result<MatchReportResponse>> RequestCorrectionAsync(Guid reportId, RequestCorrectionRequest request, CancellationToken ct)
    {
        if (!(await correctionValidator.ValidateAsync(request, ct)).IsValid)
            return Result<MatchReportResponse>.Failure(MatchReportErrors.Validation);
        return await TransitionAsync(reportId, CanRequestCorrection, (r, actor, now, _) => r.RequestCorrection(actor, request.Reason, now), ct);
    }
    public Task<Result<MatchReportResponse>> ArchiveAsync(Guid reportId, CancellationToken ct) => TransitionAsync(reportId, static a => a.IsAdmin, static (r, actor, now, _) => r.Archive(actor, now), ct);

    public async Task<Result> EnsureEditableAsync(Guid matchId, CancellationToken ct)
    {
        var aggregate = await repository.GetByMatchAggregateAsync(matchId, ct);
        return aggregate is null || aggregate.Report.Status is MatchReportStatus.DRAFT or MatchReportStatus.NEEDS_CORRECTION ? Result.Success() : Result.Failure(MatchReportErrors.WorkflowLocked);
    }
    public async Task<Result> EnsureMatchCanBeArchivedAsync(Guid matchId, CancellationToken ct)
    {
        var aggregate = await repository.GetByMatchAggregateAsync(matchId, ct);
        return aggregate is null || aggregate.Report.Status == MatchReportStatus.ARCHIVED ? Result.Success() : Result.Failure(MatchReportErrors.Conflict);
    }

    private async Task<Result<MatchReportResponse>> TransitionAsync(Guid reportId, Func<CurrentUserAccess, bool> permitted, Action<MatchReport, Guid, DateTime, MatchReportAggregate> transition, CancellationToken ct)
    {
        var access = await currentUserAccess.GetAsync(ct);
        var aggregate = await repository.GetAggregateAsync(reportId, ct);
        if (aggregate is null || !CanAccess(access, aggregate.Match.TeamId))
            return Result<MatchReportResponse>.Failure(MatchReportErrors.NotFound);
        if (!permitted(access))
            return Result<MatchReportResponse>.Failure(MatchReportErrors.Forbidden);
        if (aggregate.Match.IsArchived || aggregate.Match.Status != MatchStatus.PLAYED)
            return Result<MatchReportResponse>.Failure(MatchReportErrors.Conflict);
        try
        {
            transition(aggregate.Report, access.UserId!.Value, clock.UtcNow, aggregate);
        }
        catch (ArgumentException)
        {
            return Result<MatchReportResponse>.Failure(MatchReportErrors.Conflict);
        }
        catch (InvalidOperationException)
        {
            return Result<MatchReportResponse>.Failure(MatchReportErrors.Conflict);
        }
        await repository.SaveChangesAsync(ct);
        var read = await repository.GetReadByMatchAsync(aggregate.Match.Id, null, ct);
        return Result<MatchReportResponse>.Success(ToResponse(read!, access));
    }
    private static bool CanRead(CurrentUserAccess a) => a.IsActive && a.HasAccessProfile;
    private static bool CanEdit(CurrentUserAccess a) => CanRead(a) && (a.IsAdmin || a.PrimaryRole == StaffRole.DATA_OPERATOR);
    private static bool CanVerify(CurrentUserAccess a) => CanRead(a) && (a.IsAdmin || a.PrimaryRole == StaffRole.ANALYST && a.EffectivePermissions.CanVerifyReports);
    private static bool CanRequestCorrection(CurrentUserAccess a) => CanRead(a) && (a.IsAdmin || a.PrimaryRole == StaffRole.ANALYST);
    private static bool CanAccess(CurrentUserAccess a, Guid teamId) => a.IsAdmin || a.TeamScopeType == TeamScopeType.ALL_TEAMS || a.SelectedTeamIds.Contains(teamId);
    private static IReadOnlyCollection<Guid>? Scope(CurrentUserAccess a) => a.IsAdmin || a.TeamScopeType == TeamScopeType.ALL_TEAMS ? null : a.SelectedTeamIds;
    private static IReadOnlyCollection<MatchReportStatus> VisibleStatuses(CurrentUserAccess a) => a.IsAdmin || a.PrimaryRole is StaffRole.DATA_OPERATOR or StaffRole.ANALYST ? Enum.GetValues<MatchReportStatus>() : [MatchReportStatus.VERIFIED, MatchReportStatus.ARCHIVED];
    private static bool CanViewStatus(CurrentUserAccess a, MatchReportStatus status) => VisibleStatuses(a).Contains(status);
    private static IReadOnlyList<MatchReportAction> Actions(MatchReportReadModel x, CurrentUserAccess a)
    {
        if (!CanRead(a) || !CanAccess(a, x.Match.TeamId) || !CanViewStatus(a, x.Report.Status))
            return [];
        var actions = new List<MatchReportAction> { MatchReportAction.VIEW };
        if (x.Match.IsArchived || x.Match.Status != MatchStatus.PLAYED || x.Report.Status == MatchReportStatus.ARCHIVED)
            return actions;
        if (x.Report.Status is MatchReportStatus.DRAFT or MatchReportStatus.NEEDS_CORRECTION && CanEdit(a))
        {
            actions.Add(MatchReportAction.EDIT);
            actions.Add(MatchReportAction.SUBMIT_FOR_REVIEW);
        }
        if (x.Report.Status == MatchReportStatus.READY_FOR_REVIEW)
        {
            if (CanVerify(a))
                actions.Add(MatchReportAction.VERIFY);
            if (CanRequestCorrection(a))
                actions.Add(MatchReportAction.REQUEST_CORRECTION);
        }
        if (x.Report.Status == MatchReportStatus.VERIFIED)
        {
            if (CanRequestCorrection(a))
                actions.Add(MatchReportAction.REQUEST_CORRECTION);
            if (a.IsAdmin)
                actions.Add(MatchReportAction.ARCHIVE);
        }
        return actions;
    }
    private static MatchReportResponse ToResponse(MatchReportReadModel x, CurrentUserAccess a) => new(x.Report.Id, x.Report.MatchId, x.Report.Status, Actions(x, a), x.Report.CreatedByUserId, x.Report.CreatedAtUtc, Actor(x.Report.SubmittedByUserId, x.Report.SubmittedAtUtc), Actor(x.Report.VerifiedByUserId, x.Report.VerifiedAtUtc), Correction(x.Report), Actor(x.Report.ArchivedByUserId, x.Report.ArchivedAtUtc));
    private static MatchReportListItemResponse ToListItem(MatchReportReadModel x, CurrentUserAccess a) => new(x.Report.Id, x.Match.Id, x.Match.KickoffAtUtc, new(x.Match.TeamId, x.TeamName), new(x.Match.OpponentId, x.OpponentName), new(x.Match.CompetitionId, x.CompetitionName), x.Report.Status, Actions(x, a), Actor(x.Report.SubmittedByUserId, x.Report.SubmittedAtUtc), Actor(x.Report.VerifiedByUserId, x.Report.VerifiedAtUtc), Correction(x.Report));
    private static MatchReportActorResponse? Actor(Guid? id, DateTime? at) => id.HasValue && at.HasValue ? new(id.Value, at.Value) : null;
    private static MatchReportCorrectionResponse? Correction(MatchReport report) => report.LastCorrectionRequestedByUserId.HasValue && report.LastCorrectionRequestedAtUtc.HasValue && report.LastCorrectionReason is not null ? new(report.LastCorrectionRequestedByUserId.Value, report.LastCorrectionRequestedAtUtc.Value, report.LastCorrectionReason) : null;
}
