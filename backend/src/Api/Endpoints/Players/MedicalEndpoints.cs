using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using PlayerPerformance.Api.Authentication;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Application.Auditing;
using PlayerPerformance.Application.Authorization;
using PlayerPerformance.Domain.Auditing;
using PlayerPerformance.Domain.Medical;
using PlayerPerformance.Domain.Players;
using PlayerPerformance.Domain.Staff;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.Api.Endpoints.Players;

internal static class MedicalEndpoints
{
    public static IEndpointRouteBuilder MapMedicalEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var availability = endpoints.MapGroup("/api/player-availability").RequireAuthorization().WithTags("Player availability");
        availability.MapGet("", ListAvailabilityAsync);
        availability.MapGet("/summary", SummaryAsync);
        availability.MapGet("/{availabilityId:guid}/audit", AvailabilityAuditAsync);
        var players = endpoints.MapGroup("/api/players").RequireAuthorization();
        players.MapGet("/{playerId:guid}/availability", PlayerHistoryAsync);
        players.MapPost("/{playerId:guid}/availability", RecordAvailabilityAsync);
        var injuries = endpoints.MapGroup("/api/medical/injuries").RequireAuthorization().WithTags("Restricted medical injuries");
        endpoints.MapGet("/api/medical/injury-player-candidates", InjuryPlayerCandidatesAsync).RequireAuthorization().WithTags("Restricted medical injuries");
        injuries.MapGet("", ListInjuriesAsync);
        injuries.MapGet("/{injuryId:guid}", InjuryDetailAsync);
        injuries.MapGet("/{injuryId:guid}/revisions", InjuryRevisionsAsync);
        injuries.MapGet("/{injuryId:guid}/audit", InjuryAuditAsync);
        injuries.MapPost("", CreateInjuryAsync);
        injuries.MapPatch("/{injuryId:guid}", UpdateInjuryAsync);
        injuries.MapPost("/{injuryId:guid}/resolve", ResolveInjuryAsync);
        return endpoints;
    }
    private static bool Read(CurrentUserAccess a, Guid team) => a.IsActive && a.HasAccessProfile && (a.IsAdmin || a.TeamScopeType == TeamScopeType.ALL_TEAMS || a.SelectedTeamIds.Contains(team));
    private static bool SafeWrite(CurrentUserAccess a, Guid team) => Read(a, team) && (a.IsAdmin || a.PrimaryRole == StaffRole.MEDICAL_STAFF);
    private static bool DetailRead(CurrentUserAccess a, Guid team) => Read(a, team) && (a.IsAdmin || a.EffectivePermissions.CanViewMedicalDetails);
    private static bool InjuryWrite(CurrentUserAccess a, Guid team) => DetailRead(a, team) && (a.IsAdmin || a.PrimaryRole == StaffRole.MEDICAL_STAFF);
    private static bool ValidPage(int page, int pageSize) => page >= 1 && pageSize is >= 1 and <= 100;
    private static DateOnly Today(ISystemClock clock) => DateOnly.FromDateTime(clock.UtcNow);

    private static async Task<IResult> ListAvailabilityAsync(Guid teamId, AppDbContext db, ICurrentUserAccess access, ISystemClock clock, AvailabilityStatus? status = null, string? search = null, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        var a = await access.GetAsync(ct);
        if (!Read(a, teamId))
            return Results.Forbid();
        if (!ValidPage(page, pageSize) || search?.Length > 200)
            return Results.BadRequest();
        var today = Today(clock);
        var baseQuery = from assignment in db.PlayerTeamAssignments.AsNoTracking()
                        join player in db.Players.AsNoTracking() on assignment.PlayerId equals player.Id
                        join av in db.PlayerAvailabilities.AsNoTracking() on new { assignment.PlayerId, assignment.TeamId } equals new { av.PlayerId, av.TeamId } into avs
                        from av in avs.DefaultIfEmpty()
                        join revision in db.PlayerAvailabilityRevisions.AsNoTracking() on av.CurrentRevisionId equals revision.Id into revisions
                        from revision in revisions.DefaultIfEmpty()
                        where assignment.TeamId == teamId && assignment.StartDate <= today && (assignment.EndDate == null || assignment.EndDate >= today) && player.Status != PlayerRecordStatus.ARCHIVED
                        select new { player, revision };
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpper();
            baseQuery = baseQuery.Where(x => x.player.FirstName.ToUpper().Contains(term) || x.player.LastName.ToUpper().Contains(term) || x.player.PreferredName != null && x.player.PreferredName.ToUpper().Contains(term));
        }
        if (status.HasValue)
            baseQuery = baseQuery.Where(x => (x.revision == null ? AvailabilityStatus.UNKNOWN : x.revision.Status) == status.Value);
        var total = await baseQuery.CountAsync(ct);
        var items = await baseQuery.OrderBy(x => x.player.LastName).ThenBy(x => x.player.FirstName).ThenBy(x => x.player.Id).Skip((page - 1) * pageSize).Take(pageSize).Select(x => new
        {
            playerId = x.player.Id,
            displayName = x.player.PreferredName ?? (x.player.FirstName + " " + x.player.LastName),
            teamId,
            status = x.revision == null ? AvailabilityStatus.UNKNOWN : x.revision.Status,
            effectiveOn = x.revision == null ? (DateOnly?)null : x.revision.EffectiveOn,
            expectedReturnOn = x.revision == null ? (DateOnly?)null : x.revision.ExpectedReturnOn,
            coachVisibleNote = x.revision == null ? null : x.revision.CoachVisibleNote,
            currentRevisionId = x.revision == null ? (Guid?)null : x.revision.Id,
            revisionNumber = x.revision == null ? (int?)null : x.revision.RevisionNumber,
            recordedAtUtc = x.revision == null ? (DateTime?)null : x.revision.RecordedAtUtc,
            allowedActions = SafeWrite(a, teamId) ? new[] { "VIEW", "UPDATE" } : new[] { "VIEW" }
        }).ToListAsync(ct);
        return Results.Ok(new
        {
            items,
            page,
            pageSize,
            totalCount = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }
    private static async Task<IResult> SummaryAsync(Guid teamId, AppDbContext db, ICurrentUserAccess access, ISystemClock clock, CancellationToken ct)
    {
        var a = await access.GetAsync(ct);
        if (!Read(a, teamId))
            return Results.Forbid();
        var today = Today(clock);
        var statuses = await (from assignment in db.PlayerTeamAssignments.AsNoTracking() join player in db.Players.AsNoTracking() on assignment.PlayerId equals player.Id join av in db.PlayerAvailabilities.AsNoTracking() on new { assignment.PlayerId, assignment.TeamId } equals new { av.PlayerId, av.TeamId } into avs from av in avs.DefaultIfEmpty() join rev in db.PlayerAvailabilityRevisions.AsNoTracking() on av.CurrentRevisionId equals rev.Id into revs from rev in revs.DefaultIfEmpty() where assignment.TeamId == teamId && assignment.StartDate <= today && (assignment.EndDate == null || assignment.EndDate >= today) && player.Status != PlayerRecordStatus.ARCHIVED select rev == null ? AvailabilityStatus.UNKNOWN : rev.Status).ToListAsync(ct);
        return Results.Ok(new
        {
            totalPlayers = statuses.Count,
            availableCount = statuses.Count(x => x == AvailabilityStatus.AVAILABLE),
            limitedCount = statuses.Count(x => x == AvailabilityStatus.LIMITED),
            unavailableCount = statuses.Count(x => x == AvailabilityStatus.UNAVAILABLE),
            rehabCount = statuses.Count(x => x == AvailabilityStatus.REHAB),
            unknownCount = statuses.Count(x => x == AvailabilityStatus.UNKNOWN),
            generatedAtUtc = clock.UtcNow
        });
    }
    private static async Task<IResult> PlayerHistoryAsync(Guid playerId, AppDbContext db, ICurrentUserAccess access, Guid? teamId = null, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        if (!ValidPage(page, pageSize))
            return Results.BadRequest();
        var a = await access.GetAsync(ct);
        if (teamId.HasValue && !Read(a, teamId.Value))
            return Results.Forbid();
        var q = from av in db.PlayerAvailabilities.AsNoTracking() join rev in db.PlayerAvailabilityRevisions.AsNoTracking() on av.Id equals rev.PlayerAvailabilityId where av.PlayerId == playerId && (!teamId.HasValue || av.TeamId == teamId) select new { av, rev };
        if (!a.IsAdmin && a.TeamScopeType != TeamScopeType.ALL_TEAMS)
        {
            var scopedTeamIds = a.SelectedTeamIds;
            q = q.Where(x => scopedTeamIds.Contains(x.av.TeamId));
        }
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.rev.RecordedAtUtc).ThenByDescending(x => x.rev.Id).Skip((page - 1) * pageSize).Take(pageSize).Select(x => new { availabilityId = x.av.Id, teamId = x.av.TeamId, status = x.rev.Status, effectiveOn = x.rev.EffectiveOn, expectedReturnOn = x.rev.ExpectedReturnOn, coachVisibleNote = x.rev.CoachVisibleNote, currentRevisionId = x.rev.Id, revisionNumber = x.rev.RevisionNumber, recordedAtUtc = x.rev.RecordedAtUtc, allowedActions = SafeWrite(a, x.av.TeamId) ? new[] { "VIEW", "UPDATE" } : new[] { "VIEW" } }).ToListAsync(ct);
        return Results.Ok(new
        {
            items = rows,
            page,
            pageSize,
            totalCount = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }
    private static bool ReadScope(CurrentUserAccess a, Guid teamId) => a.IsAdmin || a.TeamScopeType == TeamScopeType.ALL_TEAMS || a.SelectedTeamIds.Contains(teamId);
    private static async Task<IResult> InjuryPlayerCandidatesAsync(Guid teamId, DateOnly occurredOn, AppDbContext db, ICurrentUserAccess access, Guid? playerId = null, string? search = null, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        if (teamId == Guid.Empty || !ValidPage(page, pageSize) || search?.Length > 200)
            return Results.BadRequest();
        var a = await access.GetAsync(ct);
        if (!InjuryWrite(a, teamId))
            return Results.Forbid();
        var query = from assignment in db.PlayerTeamAssignments.AsNoTracking()
                    join player in db.Players.AsNoTracking() on assignment.PlayerId equals player.Id
                    where assignment.TeamId == teamId && assignment.StartDate <= occurredOn && (assignment.EndDate == null || assignment.EndDate >= occurredOn) && player.Status != PlayerRecordStatus.ARCHIVED
                    select new { assignment, player };
        if (playerId.HasValue)
            query = query.Where(x => x.player.Id == playerId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpper();
            query = query.Where(x => x.player.FirstName.ToUpper().Contains(term) || x.player.LastName.ToUpper().Contains(term) || x.player.PreferredName != null && x.player.PreferredName.ToUpper().Contains(term));
        }
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.player.PreferredName ?? (x.player.FirstName + " " + x.player.LastName)).ThenBy(x => x.player.Id).Skip((page - 1) * pageSize).Take(pageSize).Select(x => new
        {
            id = x.player.Id,
            displayName = x.player.PreferredName ?? (x.player.FirstName + " " + x.player.LastName),
            preferredName = x.player.PreferredName,
            dateOfBirth = x.player.DateOfBirth,
            eligibleAssignment = new { id = x.assignment.Id, teamId = x.assignment.TeamId, startDate = x.assignment.StartDate, endDate = x.assignment.EndDate }
        }).ToListAsync(ct);
        return Results.Ok(new
        {
            items,
            page,
            pageSize,
            totalCount = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }
    private static async Task<IResult> RecordAvailabilityAsync(Guid playerId, AvailabilityRequest r, HttpContext h, IAntiforgery af, AppDbContext db, ICurrentUserAccess access, ISystemClock clock, IAuditWriter audit, CancellationToken ct)
    {
        var csrf = await AntiforgeryValidation.ValidateRequestAsync(h, af);
        if (csrf != null)
            return csrf;
        var a = await access.GetAsync(ct);
        if (a.UserId is not { } actor || !SafeWrite(a, r.TeamId))
            return Results.Forbid();
        var today = Today(clock);
        if (r.TeamId == Guid.Empty || r.EffectiveOn > today)
            return Results.UnprocessableEntity();
        var eligible = await db.Players.AnyAsync(p => p.Id == playerId && p.Status != PlayerRecordStatus.ARCHIVED, ct) && await db.Teams.AnyAsync(t => t.Id == r.TeamId && t.Status == TeamStatus.ACTIVE, ct) && await db.PlayerTeamAssignments.AnyAsync(x => x.PlayerId == playerId && x.TeamId == r.TeamId && x.StartDate <= today && (x.EndDate == null || x.EndDate >= today), ct);
        if (!eligible)
            return Results.Conflict();
        var aggregate = await db.PlayerAvailabilities.SingleOrDefaultAsync(x => x.PlayerId == playerId && x.TeamId == r.TeamId, ct);
        if (aggregate is null)
        { if (r.ExpectedCurrentRevisionId.HasValue) return Results.Conflict(); aggregate = PlayerAvailability.Create(Guid.NewGuid(), playerId, r.TeamId, clock.UtcNow); db.PlayerAvailabilities.Add(aggregate); }
        else if (aggregate.CurrentRevisionId != r.ExpectedCurrentRevisionId)
            return Results.Conflict();
        var current = aggregate.CurrentRevisionId == Guid.Empty ? null : await db.PlayerAvailabilityRevisions.SingleAsync(x => x.Id == aggregate.CurrentRevisionId, ct);
        if (current is not null && r.EffectiveOn < current.EffectiveOn)
            return Results.Conflict();
        try
        {
            var revision = PlayerAvailabilityRevision.Create(Guid.NewGuid(), aggregate.Id, (current?.RevisionNumber ?? 0) + 1, r.Status, r.EffectiveOn, r.ExpectedReturnOn, r.CoachVisibleNote, actor, clock.UtcNow);
            if (current is not null && Same(current, revision))
                return Results.Ok(new { availabilityId = aggregate.Id, currentRevisionId = current.Id, revisionNumber = current.RevisionNumber });
            aggregate.SetCurrentRevision(revision.Id);
            db.PlayerAvailabilityRevisions.Add(revision);
            audit.Add(AuditPayload.Create(actor, AuditActions.PlayerAvailabilityRecorded, AuditEntityTypes.PlayerAvailability, aggregate.Id, clock.UtcNow, metadata: new { playerId, r.TeamId, revision.RevisionNumber, changedFields = new[] { "status", "effectiveOn", "expectedReturnOn", "coachVisibleNote" } }));
            await db.SaveChangesAsync(ct);
            return Results.Ok(new
            {
                availabilityId = aggregate.Id,
                currentRevisionId = revision.Id,
                revisionNumber = revision.RevisionNumber
            });
        }
        catch (ArgumentOutOfRangeException)
        {
            return Results.UnprocessableEntity();
        }
    }
    private static bool Same(PlayerAvailabilityRevision a, PlayerAvailabilityRevision b) => a.Status == b.Status && a.EffectiveOn == b.EffectiveOn && a.ExpectedReturnOn == b.ExpectedReturnOn && a.CoachVisibleNote == b.CoachVisibleNote;
    private static async Task<IResult> ListInjuriesAsync(AppDbContext db, ICurrentUserAccess access, Guid? teamId = null, Guid? playerId = null, InjuryStatus? status = null, DateOnly? occurredFrom = null, DateOnly? occurredTo = null, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        if (!ValidPage(page, pageSize) || occurredFrom > occurredTo)
            return Results.BadRequest();
        var a = await access.GetAsync(ct);
        if (!a.IsActive || !a.HasAccessProfile || !(a.IsAdmin || a.EffectivePermissions.CanViewMedicalDetails))
            return Results.Forbid();
        if (teamId.HasValue && !DetailRead(a, teamId.Value))
            return Results.Forbid();
        var q = db.InjuryRecords.AsNoTracking().Where(x => (!teamId.HasValue || x.TeamId == teamId) && (!playerId.HasValue || x.PlayerId == playerId) && (!status.HasValue || x.Status == status) && (!occurredFrom.HasValue || x.OccurredOn >= occurredFrom) && (!occurredTo.HasValue || x.OccurredOn <= occurredTo));
        if (!a.IsAdmin && a.TeamScopeType != TeamScopeType.ALL_TEAMS)
        {
            var scopedTeamIds = a.SelectedTeamIds;
            q = q.Where(x => scopedTeamIds.Contains(x.TeamId));
        }
        var total = await q.CountAsync(ct);
        var rows = await q.OrderBy(x => x.Status == InjuryStatus.RESOLVED).ThenByDescending(x => x.OccurredOn).ThenByDescending(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).Select(x => new { id = x.Id, playerId = x.PlayerId, teamId = x.TeamId, x.OccurredOn, x.Status, x.ResolvedOn }).ToListAsync(ct);
        return Results.Ok(new
        {
            items = rows,
            page,
            pageSize,
            totalCount = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }
    private static async Task<IResult> InjuryDetailAsync(Guid injuryId, AppDbContext db, ICurrentUserAccess access, CancellationToken ct)
    {
        var injury = await db.InjuryRecords.AsNoTracking().SingleOrDefaultAsync(x => x.Id == injuryId, ct);
        if (injury is null)
            return Results.NotFound();
        var a = await access.GetAsync(ct);
        if (!DetailRead(a, injury.TeamId))
            return Results.NotFound();
        var revision = await db.InjuryRecordRevisions.AsNoTracking().SingleAsync(x => x.Id == injury.CurrentRevisionId, ct);
        var count = await db.InjuryRecordRevisions.CountAsync(x => x.InjuryRecordId == injuryId, ct);
        return Results.Ok(new
        {
            injury.Id,
            injury.PlayerId,
            injury.TeamId,
            injury.OccurredOn,
            injury.Status,
            injury.ResolvedOn,
            injury.ResolvedByUserId,
            currentRevision = revision,
            revisionCount = count,
            allowedActions = InjuryWrite(a, injury.TeamId) ? injury.Status == InjuryStatus.OPEN ? new[] { "VIEW", "UPDATE", "RESOLVE" } : new[] { "VIEW", "UPDATE" } : new[] { "VIEW" }
        });
    }
    private static async Task<IResult> InjuryRevisionsAsync(Guid injuryId, AppDbContext db, ICurrentUserAccess access, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        if (!ValidPage(page, pageSize))
            return Results.BadRequest();
        var injury = await db.InjuryRecords.AsNoTracking().SingleOrDefaultAsync(x => x.Id == injuryId, ct);
        if (injury is null || !DetailRead(await access.GetAsync(ct), injury.TeamId))
            return Results.NotFound();
        var q = db.InjuryRecordRevisions.AsNoTracking().Where(x => x.InjuryRecordId == injuryId);
        var total = await q.CountAsync(ct);
        return Results.Ok(new { items = await q.OrderByDescending(x => x.RevisionNumber).ThenByDescending(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct), page, pageSize, totalCount = total });
    }
    private static async Task<IResult> CreateInjuryAsync(CreateInjuryRequest r, HttpContext h, IAntiforgery af, AppDbContext db, ICurrentUserAccess access, ISystemClock clock, IAuditWriter audit, CancellationToken ct)
    {
        var csrf = await AntiforgeryValidation.ValidateRequestAsync(h, af);
        if (csrf != null)
            return csrf;
        var a = await access.GetAsync(ct);
        if (a.UserId is not { } actor || !InjuryWrite(a, r.TeamId))
            return Results.Forbid();
        var today = Today(clock);
        if (r.PlayerId == Guid.Empty || r.TeamId == Guid.Empty || r.OccurredOn > today)
            return Results.UnprocessableEntity();
        var valid = await db.Players.AnyAsync(x => x.Id == r.PlayerId && x.Status != PlayerRecordStatus.ARCHIVED, ct) && await db.Teams.AnyAsync(x => x.Id == r.TeamId && x.Status == TeamStatus.ACTIVE, ct) && await db.PlayerTeamAssignments.AnyAsync(x => x.PlayerId == r.PlayerId && x.TeamId == r.TeamId && x.StartDate <= r.OccurredOn && (x.EndDate == null || x.EndDate >= r.OccurredOn), ct);
        if (!valid)
            return Results.Conflict();
        try
        {
            var injury = InjuryRecord.Create(Guid.NewGuid(), r.PlayerId, r.TeamId, r.OccurredOn, actor, clock.UtcNow);
            var revision = InjuryRecordRevision.Create(Guid.NewGuid(), injury.Id, 1, r.BodyArea, r.Diagnosis, r.RestrictedNotes, actor, clock.UtcNow);
            injury.SetCurrentRevision(revision.Id);
            db.InjuryRecords.Add(injury);
            db.InjuryRecordRevisions.Add(revision);
            audit.Add(AuditPayload.Create(actor, AuditActions.InjuryRecordCreated, AuditEntityTypes.InjuryRecord, injury.Id, clock.UtcNow, metadata: new
            {
                r.PlayerId,
                r.TeamId,
                r.OccurredOn,
                changedFields = new[] { "bodyArea", "diagnosis", "restrictedNotes" }
            }));
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/medical/injuries/{injury.Id}", new { injury.Id });
        }
        catch (ArgumentOutOfRangeException)
        {
            return Results.UnprocessableEntity();
        }
    }
    private static async Task<IResult> UpdateInjuryAsync(Guid injuryId, UpdateInjuryRequest r, HttpContext h, IAntiforgery af, AppDbContext db, ICurrentUserAccess access, ISystemClock clock, IAuditWriter audit, CancellationToken ct)
    {
        var csrf = await AntiforgeryValidation.ValidateRequestAsync(h, af);
        if (csrf != null)
            return csrf;
        var injury = await db.InjuryRecords.SingleOrDefaultAsync(x => x.Id == injuryId, ct);
        if (injury is null)
            return Results.NotFound();
        var a = await access.GetAsync(ct);
        if (a.UserId is not { } actor || !InjuryWrite(a, injury.TeamId))
            return Results.Forbid();
        if (injury.CurrentRevisionId != r.ExpectedCurrentRevisionId)
            return Results.Conflict();
        var current = await db.InjuryRecordRevisions.SingleAsync(x => x.Id == injury.CurrentRevisionId, ct);
        try
        {
            var revision = InjuryRecordRevision.Create(Guid.NewGuid(), injuryId, current.RevisionNumber + 1, r.BodyArea, r.Diagnosis, r.RestrictedNotes, actor, clock.UtcNow);
            if (current.BodyArea == revision.BodyArea && current.Diagnosis == revision.Diagnosis && current.RestrictedNotes == revision.RestrictedNotes)
                return Results.Ok(new { injury.Id, currentRevisionId = current.Id, revisionNumber = current.RevisionNumber });
            injury.SetCurrentRevision(revision.Id);
            db.InjuryRecordRevisions.Add(revision);
            audit.Add(AuditPayload.Create(actor, AuditActions.InjuryRecordRevised, AuditEntityTypes.InjuryRecord, injuryId, clock.UtcNow, metadata: new
            {
                changedFields = new[] { "bodyArea", "diagnosis", "restrictedNotes" }
            }));
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { injury.Id, currentRevisionId = revision.Id, revisionNumber = revision.RevisionNumber });
        }
        catch (ArgumentOutOfRangeException)
        {
            return Results.UnprocessableEntity();
        }
    }
    private static async Task<IResult> ResolveInjuryAsync(Guid injuryId, ResolveInjuryRequest r, HttpContext h, IAntiforgery af, AppDbContext db, ICurrentUserAccess access, ISystemClock clock, IAuditWriter audit, CancellationToken ct)
    {
        var csrf = await AntiforgeryValidation.ValidateRequestAsync(h, af);
        if (csrf != null)
            return csrf;
        var injury = await db.InjuryRecords.SingleOrDefaultAsync(x => x.Id == injuryId, ct);
        if (injury is null)
            return Results.NotFound();
        var a = await access.GetAsync(ct);
        if (a.UserId is not { } actor || !InjuryWrite(a, injury.TeamId))
            return Results.Forbid();
        if (injury.CurrentRevisionId != r.ExpectedCurrentRevisionId || r.ResolvedOn > Today(clock))
            return Results.Conflict();
        try
        {
            injury.Resolve(r.ResolvedOn, actor, clock.UtcNow);
            audit.Add(AuditPayload.Create(actor, AuditActions.InjuryRecordResolved, AuditEntityTypes.InjuryRecord, injuryId, clock.UtcNow, metadata: new { r.ResolvedOn }));
            await db.SaveChangesAsync(ct);
            return Results.Ok(new
            {
                injury.Id,
                injury.Status,
                injury.ResolvedOn
            });
        }
        catch (InvalidOperationException)
        {
            return Results.Conflict();
        }
    }
    private static async Task<IResult> AvailabilityAuditAsync(Guid availabilityId, AppDbContext db, ICurrentUserAccess access, IAuditHistoryRepository audits, int page = 1, int pageSize = 25, string? action = null, DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default)
    {
        var item = await db.PlayerAvailabilities.AsNoTracking().SingleOrDefaultAsync(x => x.Id == availabilityId, ct);
        if (item is null || !Read(await access.GetAsync(ct), item.TeamId))
            return Results.NotFound();
        var q = new AuditHistoryQuery(page, pageSize, action, dateFrom, dateTo);
        return !AuditHistoryValidation.IsValid(q) ? Results.BadRequest() : Results.Ok(await audits.ListAsync(AuditEntityTypes.PlayerAvailability, availabilityId, q, ct));
    }
    private static async Task<IResult> InjuryAuditAsync(Guid injuryId, AppDbContext db, ICurrentUserAccess access, IAuditHistoryRepository audits, int page = 1, int pageSize = 25, string? action = null, DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default)
    {
        var item = await db.InjuryRecords.AsNoTracking().SingleOrDefaultAsync(x => x.Id == injuryId, ct);
        if (item is null || !DetailRead(await access.GetAsync(ct), item.TeamId))
            return Results.NotFound();
        var q = new AuditHistoryQuery(page, pageSize, action, dateFrom, dateTo);
        return !AuditHistoryValidation.IsValid(q) ? Results.BadRequest() : Results.Ok(await audits.ListAsync(AuditEntityTypes.InjuryRecord, injuryId, q, ct));
    }
    internal sealed record AvailabilityRequest(Guid TeamId, AvailabilityStatus Status, DateOnly EffectiveOn, DateOnly? ExpectedReturnOn, string? CoachVisibleNote, Guid? ExpectedCurrentRevisionId);
    internal sealed record CreateInjuryRequest(Guid PlayerId, Guid TeamId, DateOnly OccurredOn, string? BodyArea, string? Diagnosis, string? RestrictedNotes);
    internal sealed record UpdateInjuryRequest(string? BodyArea, string? Diagnosis, string? RestrictedNotes, Guid ExpectedCurrentRevisionId);
    internal sealed record ResolveInjuryRequest(DateOnly ResolvedOn, Guid ExpectedCurrentRevisionId);
}
