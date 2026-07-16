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
        return Results.Ok(await db.TrainingSessionParticipants.AsNoTracking().Where(x => x.TrainingSessionId == id && x.RemovedAtUtc == null).ToListAsync(ct));
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
        return Results.Ok(await q.ToListAsync(ct));
    }
    private static async Task<IResult> AuditAsync(Guid id, AppDbContext db, ICurrentUserAccess access, IAuditHistoryRepository audits, CancellationToken ct)
    {
        var s = await db.TrainingSessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (s is null || !Read(await access.GetAsync(ct), s.TeamId))
            return Results.NotFound();
        return Results.Ok(await audits.ListAsync(AuditEntityTypes.TrainingSession, id, new(), ct));
    }
    internal sealed record CreateTrainingSessionRequest(Guid TeamId, DateOnly SessionDate, DateTime? StartsAtUtc, DateTime? EndsAtUtc, string Title, string? Location, string? Description); internal sealed record AddParticipantRequest(Guid PlayerId);
}
