using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayerPerformance.Api.Authentication;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Application.Authorization;
using PlayerPerformance.Application.Auditing;
using PlayerPerformance.Domain.Auditing;
using PlayerPerformance.Domain.Staff;
using PlayerPerformance.Domain.Training;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.Api.Endpoints.Training;

internal static class TrainingSessionEndpoints
{
    public static IEndpointRouteBuilder MapTrainingSessionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var g = endpoints.MapGroup("/api/training-sessions").RequireAuthorization().WithTags("Training sessions");
        g.MapGet("", ListAsync);
        g.MapGet("/{id:guid}", GetAsync);
        g.MapPost("", CreateAsync);
        g.MapPatch("/{id:guid}", UpdateAsync);
        g.MapPost("/{id:guid}/complete", CompleteAsync);
        g.MapPost("/{id:guid}/cancel", CancelAsync);
        g.MapGet("/{id:guid}/participants", ParticipantsAsync);
        g.MapGet("/{id:guid}/participant-candidates", ParticipantCandidatesAsync);
        g.MapPost("/{id:guid}/participants", AddParticipantAsync);
        g.MapDelete("/{id:guid}/participants/{participantId:guid}", RemoveParticipantAsync);
        g.MapGet("/{id:guid}/workloads", WorkloadsAsync);
        g.MapGet("/{id:guid}/audit", AuditAsync);
        endpoints.MapGet("/api/matches/{matchId:guid}/physical-workloads", MatchWorkloadsAsync).RequireAuthorization();
        endpoints.MapGet("/api/players/{playerId:guid}/physical-workloads", PlayerWorkloadsAsync).RequireAuthorization();
        return endpoints;
    }
    private static bool Read(CurrentUserAccess a, Guid team) => a.IsActive && a.HasAccessProfile && (a.IsAdmin || a.TeamScopeType == TeamScopeType.ALL_TEAMS || a.SelectedTeamIds.Contains(team));
    private static bool Write(CurrentUserAccess a, Guid team) => Read(a, team) && (a.IsAdmin || a.PrimaryRole == StaffRole.DATA_OPERATOR);
    private static async Task<IResult> ListAsync(AppDbContext db, ICurrentUserAccess access, Guid? teamId, TrainingSessionStatus? status, DateOnly? dateFrom, DateOnly? dateTo, string? search, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        var a = await access.GetAsync(ct);
        if (teamId.HasValue && !Read(a, teamId.Value))
            return Results.Forbid();
        var q = db.TrainingSessions.AsNoTracking().AsQueryable();
        if (teamId.HasValue)
            q = q.Where(x => x.TeamId == teamId);
        else if (!a.IsAdmin && a.TeamScopeType != TeamScopeType.ALL_TEAMS)
            q = q.Where(x => a.SelectedTeamIds.Contains(x.TeamId));
        if (status.HasValue)
            q = q.Where(x => x.Status == status);
        if (dateFrom.HasValue)
            q = q.Where(x => x.SessionDate >= dateFrom);
        if (dateTo.HasValue)
            q = q.Where(x => x.SessionDate <= dateTo);
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(x => x.Title.Contains(search) || x.Location != null && x.Location.Contains(search));
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.SessionDate).ThenByDescending(x => x.StartsAtUtc).ThenByDescending(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Results.Ok(new
        {
            items = rows,
            page,
            pageSize,
            totalCount = total
        });
    }
    private static async Task<IResult> GetAsync(Guid id, AppDbContext db, ICurrentUserAccess access, CancellationToken ct)
    {
        var s = await db.TrainingSessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (s is null)
            return Results.NotFound();
        return Read(await access.GetAsync(ct), s.TeamId) ? Results.Ok(s) : Results.NotFound();
    }
    private static async Task<IResult> CreateAsync(CreateTrainingSessionRequest r, HttpContext h, IAntiforgery af, AppDbContext db, ICurrentUserAccess access, ISystemClock clock, IAuditWriter audit, CancellationToken ct)
    {
        var f = await AntiforgeryValidation.ValidateRequestAsync(h, af);
        if (f != null)
            return f;
        var a = await access.GetAsync(ct);
        if (a.UserId is not { } actor || !Write(a, r.TeamId))
            return Results.Forbid();
        try
        {
            var s = TrainingSession.Create(Guid.NewGuid(), r.TeamId, r.SessionDate, r.StartsAtUtc, r.EndsAtUtc, r.Title, r.Location, r.Description, actor, clock.UtcNow);
            db.TrainingSessions.Add(s);
            audit.Add(AuditPayload.Create(actor, AuditActions.TrainingSessionCreated, AuditEntityTypes.TrainingSession, s.Id, clock.UtcNow, metadata: new { r.TeamId, r.SessionDate }));
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/training-sessions/{s.Id}", s);
        }
        catch (ArgumentException)
        {
            return Results.UnprocessableEntity();
        }
    }
    private static async Task<IResult> UpdateAsync(Guid id, CreateTrainingSessionRequest r, HttpContext h, IAntiforgery af, AppDbContext db, ICurrentUserAccess access, ISystemClock clock, IAuditWriter audit, CancellationToken ct)
    {
        var f = await AntiforgeryValidation.ValidateRequestAsync(h, af);
        if (f != null)
            return f;
        var s = await db.TrainingSessions.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (s is null)
            return Results.NotFound();
        var a = await access.GetAsync(ct);
        if (a.UserId is not { } actor || !Write(a, s.TeamId))
            return Results.Forbid();
        try
        {
            if (s.Update(r.SessionDate, r.StartsAtUtc, r.EndsAtUtc, r.Title, r.Location, r.Description, clock.UtcNow))
            { audit.Add(AuditPayload.Create(actor, AuditActions.TrainingSessionUpdated, AuditEntityTypes.TrainingSession, id, clock.UtcNow)); await db.SaveChangesAsync(ct); }
            return Results.Ok(s);
        }
        catch
        {
            return Results.Conflict();
        }
    }
    private static async Task<IResult> CompleteAsync(Guid id, HttpContext h, IAntiforgery af, AppDbContext db, ICurrentUserAccess access, ISystemClock clock, IAuditWriter audit, CancellationToken ct) => await ChangeAsync(id, true, h, af, db, access, clock, audit, ct);
    private static async Task<IResult> CancelAsync(Guid id, HttpContext h, IAntiforgery af, AppDbContext db, ICurrentUserAccess access, ISystemClock clock, IAuditWriter audit, CancellationToken ct) => await ChangeAsync(id, false, h, af, db, access, clock, audit, ct);
    private static async Task<IResult> ChangeAsync(Guid id, bool complete, HttpContext h, IAntiforgery af, AppDbContext db, ICurrentUserAccess access, ISystemClock clock, IAuditWriter audit, CancellationToken ct)
    {
        var f = await AntiforgeryValidation.ValidateRequestAsync(h, af);
        if (f != null)
            return f;
        var s = await db.TrainingSessions.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (s is null)
            return Results.NotFound();
        var a = await access.GetAsync(ct);
        if (a.UserId is not { } actor || !Write(a, s.TeamId))
            return Results.Forbid();
        try
        {
            if (complete)
                s.Complete(actor, clock.UtcNow);
            else
                s.Cancel(actor, clock.UtcNow);
            audit.Add(AuditPayload.Create(actor, complete ? AuditActions.TrainingSessionCompleted : AuditActions.TrainingSessionCancelled, AuditEntityTypes.TrainingSession, id, clock.UtcNow));
            await db.SaveChangesAsync(ct);
            return Results.Ok(s);
        }
        catch
        {
            return Results.Conflict();
        }
    }
    private static async Task<IResult> ParticipantsAsync(Guid id, AppDbContext db, ICurrentUserAccess access, CancellationToken ct)
    {
        var s = await db.TrainingSessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (s is null || !Read(await access.GetAsync(ct), s.TeamId))
            return Results.NotFound();
        var participants = await (from participant in db.TrainingSessionParticipants.AsNoTracking()
                                  join player in db.Players.AsNoTracking() on participant.PlayerId equals player.Id
                                  where participant.TrainingSessionId == id && participant.RemovedAtUtc == null
                                  orderby player.LastName, player.FirstName
                                  select new ParticipantResponse(participant.Id, participant.TrainingSessionId, participant.PlayerId, player.FirstName + " " + player.LastName, player.PreferredName, participant.RemovedAtUtc))
            .ToListAsync(ct);
        return Results.Ok(participants);
    }
    private static async Task<IResult> ParticipantCandidatesAsync(Guid id, AppDbContext db, ICurrentUserAccess access, string? search = null, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        if (page < 1 || pageSize is < 1 or > 100 || search?.Length > 200)
            return Results.BadRequest();
        var session = await db.TrainingSessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        var actor = await access.GetAsync(ct);
        if (session is null)
            return Results.NotFound();
        if (!Write(actor, session.TeamId))
            return Results.Forbid();

        var activeParticipantIds = db.TrainingSessionParticipants.AsNoTracking()
            .Where(x => x.TrainingSessionId == id && x.RemovedAtUtc == null)
            .Select(x => x.PlayerId);
        var candidates = from assignment in db.PlayerTeamAssignments.AsNoTracking()
                         join player in db.Players.AsNoTracking() on assignment.PlayerId equals player.Id
                         where assignment.TeamId == session.TeamId
                               && assignment.StartDate <= session.SessionDate
                               && (assignment.EndDate == null || assignment.EndDate >= session.SessionDate)
                               && !activeParticipantIds.Contains(player.Id)
                         select new { player.Id, player.FirstName, player.LastName, player.PreferredName, assignment.StartDate, assignment.EndDate };
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpper();
            candidates = candidates.Where(x => x.FirstName.ToUpper().Contains(term) || x.LastName.ToUpper().Contains(term) || x.PreferredName != null && x.PreferredName.ToUpper().Contains(term));
        }
        var total = await candidates.Select(x => x.Id).Distinct().CountAsync(ct);
        var items = await candidates.GroupBy(x => new { x.Id, x.FirstName, x.LastName, x.PreferredName })
            .OrderBy(x => x.Key.LastName).ThenBy(x => x.Key.FirstName).ThenBy(x => x.Key.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new { x.Key.Id, x.Key.FirstName, x.Key.LastName, x.Key.PreferredName, assignmentStartDate = x.Min(y => y.StartDate), assignmentEndDate = x.Max(y => y.EndDate) })
            .ToListAsync(ct);
        return Results.Ok(new { items, page, pageSize, totalCount = total });
    }
    private static async Task<IResult> AddParticipantAsync(Guid id, AddParticipantRequest r, HttpContext h, IAntiforgery af, AppDbContext db, ICurrentUserAccess access, ISystemClock clock, IAuditWriter audit, CancellationToken ct)
    {
        var f = await AntiforgeryValidation.ValidateRequestAsync(h, af);
        if (f != null)
            return f;
        var s = await db.TrainingSessions.SingleOrDefaultAsync(x => x.Id == id, ct);
        var a = await access.GetAsync(ct);
        if (s is null)
            return Results.NotFound();
        if (a.UserId is not { } actor || !Write(a, s.TeamId))
            return Results.Forbid();
        if (s.Status == TrainingSessionStatus.CANCELLED || await db.TrainingSessionParticipants.AnyAsync(x => x.TrainingSessionId == id && x.PlayerId == r.PlayerId && x.RemovedAtUtc == null, ct))
            return Results.Conflict();
        var eligible = await db.PlayerTeamAssignments.AnyAsync(x => x.PlayerId == r.PlayerId && x.TeamId == s.TeamId && x.StartDate <= s.SessionDate && (x.EndDate == null || x.EndDate >= s.SessionDate), ct);
        if (!eligible)
            return Results.UnprocessableEntity();
        var p = TrainingSessionParticipant.Create(Guid.NewGuid(), id, r.PlayerId, actor, clock.UtcNow);
        db.TrainingSessionParticipants.Add(p);
        audit.Add(AuditPayload.Create(actor, AuditActions.TrainingSessionParticipantAdded, AuditEntityTypes.TrainingSession, id, clock.UtcNow, metadata: new { r.PlayerId }));
        await db.SaveChangesAsync(ct);
        return Results.Created($"/api/training-sessions/{id}/participants/{p.Id}", p);
    }
    private static async Task<IResult> RemoveParticipantAsync(Guid id, Guid participantId, HttpContext h, IAntiforgery af, AppDbContext db, ICurrentUserAccess access, ISystemClock clock, IAuditWriter audit, CancellationToken ct)
    {
        var f = await AntiforgeryValidation.ValidateRequestAsync(h, af);
        if (f != null)
            return f;
        var s = await db.TrainingSessions.SingleOrDefaultAsync(x => x.Id == id, ct);
        var p = await db.TrainingSessionParticipants.SingleOrDefaultAsync(x => x.Id == participantId && x.TrainingSessionId == id, ct);
        var a = await access.GetAsync(ct);
        if (s is null || p is null)
            return Results.NotFound();
        if (a.UserId is not { } actor || !Write(a, s.TeamId))
            return Results.Forbid();
        if (await db.PlayerPhysicalWorkloads.AnyAsync(x => x.TrainingSessionParticipantId == participantId, ct))
            return Results.Conflict();
        try
        {
            p.Remove(actor, clock.UtcNow);
            audit.Add(AuditPayload.Create(actor, AuditActions.TrainingSessionParticipantRemoved, AuditEntityTypes.TrainingSession, id, clock.UtcNow, metadata: new { p.PlayerId }));
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }
        catch
        {
            return Results.Conflict();
        }
    }
    private static async Task<IResult> WorkloadsAsync(Guid id, AppDbContext db, ICurrentUserAccess access, CancellationToken ct) => await WorkloadQuery(db, access, ct, x => x.TrainingSessionParticipantId != null && db.TrainingSessionParticipants.Where(p => p.TrainingSessionId == id).Select(p => p.Id).Contains(x.TrainingSessionParticipantId.Value));
    private static async Task<IResult> MatchWorkloadsAsync(Guid matchId, AppDbContext db, ICurrentUserAccess access, CancellationToken ct) => await WorkloadQuery(db, access, ct, x => x.PlayerMatchAppearanceId != null && db.PlayerMatchAppearances.Where(a => a.MatchId == matchId).Select(a => a.Id).Contains(x.PlayerMatchAppearanceId.Value));
    private static async Task<IResult> PlayerWorkloadsAsync(Guid playerId, AppDbContext db, ICurrentUserAccess access, CancellationToken ct) => await WorkloadQuery(db, access, ct, x => x.PlayerId == playerId);
    private static async Task<IResult> WorkloadQuery(AppDbContext db, ICurrentUserAccess access, CancellationToken ct, System.Linq.Expressions.Expression<Func<PlayerPerformance.Domain.Physical.PlayerPhysicalWorkload, bool>> predicate)
    {
        var a = await access.GetAsync(ct);
        var q = db.PlayerPhysicalWorkloads.AsNoTracking().Where(predicate);
        if (!a.IsAdmin && a.TeamScopeType != TeamScopeType.ALL_TEAMS)
            q = q.Where(x => a.SelectedTeamIds.Contains(x.TeamId));
        var sourceRows = await (from workload in q
                                join player in db.Players.AsNoTracking() on workload.PlayerId equals player.Id
                                join revision in db.PhysicalWorkloadRevisions.AsNoTracking() on workload.CurrentRevisionId equals revision.Id
                                orderby workload.OccurredOn descending, player.LastName, player.FirstName
                                select new
                                {
                                    workload.Id,
                                    workload.PlayerId,
                                    PlayerName = player.FirstName + " " + player.LastName,
                                    workload.TeamId,
                                    workload.OccurredOn,
                                    workload.TrainingSessionParticipantId,
                                    workload.PlayerMatchAppearanceId,
                                    CurrentRevisionId = revision.Id,
                                    revision.RevisionNumber,
                                    revision.ImportJobId,
                                    revision.SourceSystem,
                                    revision.ProcessorKey,
                                    revision.ProcessorVersion,
                                    revision.RecordedAtUtc
                                }).ToListAsync(ct);
        var revisionIds = sourceRows.Select(x => x.CurrentRevisionId).ToArray();
        var metricsByRevision = (await db.PhysicalMetricValues.AsNoTracking()
                .Where(value => revisionIds.Contains(value.PhysicalWorkloadRevisionId))
                .OrderBy(value => value.MetricCode)
                .Select(value => new
                {
                    value.PhysicalWorkloadRevisionId,
                    Metric = new PhysicalMetricValueResponse(value.MetricCode, value.Value, value.UnitCode, value.ThresholdValue, value.ThresholdUnitCode, value.ThresholdDirection, value.ThresholdScope, value.MethodKey, value.MethodVersion)
                }).ToListAsync(ct))
            .GroupBy(x => x.PhysicalWorkloadRevisionId)
            .ToDictionary(x => x.Key, x => (IReadOnlyList<PhysicalMetricValueResponse>)x.Select(value => value.Metric).ToList());
        var rows = sourceRows.Select(x => new PhysicalWorkloadResponse(x.Id, x.PlayerId, x.PlayerName, x.TeamId, x.OccurredOn, x.TrainingSessionParticipantId, x.PlayerMatchAppearanceId, x.CurrentRevisionId, x.RevisionNumber, x.ImportJobId, x.SourceSystem, x.ProcessorKey, x.ProcessorVersion, x.RecordedAtUtc, metricsByRevision.GetValueOrDefault(x.CurrentRevisionId, Array.Empty<PhysicalMetricValueResponse>()))).ToList();
        return Results.Ok(rows);
    }
    private static async Task<IResult> AuditAsync(Guid id, AppDbContext db, ICurrentUserAccess access, IAuditHistoryRepository audits, CancellationToken ct)
    {
        var s = await db.TrainingSessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (s is null || !Read(await access.GetAsync(ct), s.TeamId))
            return Results.NotFound();
        return Results.Ok(await audits.ListAsync(AuditEntityTypes.TrainingSession, id, new(), ct));
    }
    internal sealed record CreateTrainingSessionRequest(Guid TeamId, DateOnly SessionDate, DateTime? StartsAtUtc, DateTime? EndsAtUtc, string Title, string? Location, string? Description); internal sealed record AddParticipantRequest(Guid PlayerId);
    private sealed record ParticipantResponse(Guid Id, Guid TrainingSessionId, Guid PlayerId, string PlayerName, string? PreferredName, DateTime? RemovedAtUtc);
    private sealed record PhysicalWorkloadResponse(Guid Id, Guid PlayerId, string PlayerName, Guid TeamId, DateOnly OccurredOn, Guid? TrainingSessionParticipantId, Guid? PlayerMatchAppearanceId, Guid CurrentRevisionId, int RevisionNumber, Guid? ImportJobId, string? SourceSystem, string? ProcessorKey, string? ProcessorVersion, DateTime RecordedAtUtc, IReadOnlyList<PhysicalMetricValueResponse> Metrics);
    private sealed record PhysicalMetricValueResponse(string MetricCode, decimal Value, PlayerPerformance.Domain.Physical.PhysicalMetricUnit UnitCode, decimal? ThresholdValue, PlayerPerformance.Domain.Physical.PhysicalMetricUnit? ThresholdUnitCode, PlayerPerformance.Domain.Physical.ThresholdDirection? ThresholdDirection, PlayerPerformance.Domain.Physical.ThresholdScope? ThresholdScope, string? MethodKey, string? MethodVersion);
}
