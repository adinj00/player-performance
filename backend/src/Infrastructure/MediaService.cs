using Microsoft.EntityFrameworkCore;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Application.Auditing;
using PlayerPerformance.Application.Authorization;
using PlayerPerformance.Application.Media;
using PlayerPerformance.Domain.Auditing;
using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Media;
using PlayerPerformance.Infrastructure.Persistence;
namespace PlayerPerformance.Infrastructure.Media;

internal sealed class MediaService(AppDbContext db, ICurrentUserAccess current, ITeamAccessService teamAccess, ISystemClock clock, IAuditWriter audit) : IMediaService
{
    private static readonly Error NotFound = new("not_found", "The requested media is not available."),
    Forbidden = new("forbidden", "You do not have access to this media."),
    Validation = new("validation_failed", "One or more fields are invalid."),
    Conflict = new("media_conflict", "The requested media change is not allowed.");
    public async Task<Result<PagedMediaResponse>> ListAsync(MediaListQuery q, CancellationToken ct)
    {
        var a = await current.GetAsync(ct);
        if (!a.IsActive || !a.HasAccessProfile)
            return Result<PagedMediaResponse>.Failure(Forbidden);
        if (q.Page < 1 || q.PageSize is < 1 or > 100 || q.Search?.Length > 200)
            return Result<PagedMediaResponse>.Failure(Validation);
        if (q.TeamId.HasValue && !await teamAccess.CanAccessAsync(q.TeamId.Value, ct))
            return Result<PagedMediaResponse>.Failure(Forbidden);
        if (q.IncludeArchived && !a.IsAdmin)
            return Result<PagedMediaResponse>.Failure(Forbidden);
        var s = db.MediaItems.AsNoTracking().Where(x => (a.IsAdmin || a.TeamScopeType == Domain.Staff.TeamScopeType.ALL_TEAMS || a.SelectedTeamIds.Contains(x.TeamId)) && (q.IncludeArchived || !x.IsArchived));
        if (q.TeamId.HasValue)
            s = s.Where(x => x.TeamId == q.TeamId);
        if (q.SourceType.HasValue)
            s = s.Where(x => x.SourceType == q.SourceType);
        if (q.Category.HasValue)
            s = s.Where(x => x.Category == q.Category);
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var z = q.Search.Trim().ToUpper();
            s = s.Where(x => x.Title.ToUpper().Contains(z) || (x.Description != null && x.Description.ToUpper().Contains(z)));
        }
        var total = await s.CountAsync(ct);
        var ids = await s.OrderByDescending(x => x.CreatedAtUtc).ThenBy(x => x.Id).Skip((q.Page - 1) * q.PageSize).Take(q.PageSize).Select(x => x.Id).ToListAsync(ct);
        var items = new List<MediaResponse>();
        foreach (var id in ids)
        {
            var r = await ReadAsync(id, ct);
            if (r is not null)
                items.Add(r);
        }
        return Result<PagedMediaResponse>.Success(new(items, q.Page, q.PageSize, total, (int)Math.Ceiling(total / (double)q.PageSize)));
    }
    public async Task<MediaResponse?> GetAsync(Guid id, CancellationToken ct)
    {
        var item = await db.MediaItems.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item is null || !await CanRead(item, ct))
            return null;
        return await ReadAsync(id, ct);
    }
    public async Task<Result<MediaResponse>> CreateExternalAsync(CreateExternalMediaReferenceRequest r, CancellationToken ct)
    {
        var a = await current.GetAsync(ct);
        if (!CanMutate(a) || !await teamAccess.CanAccessAsync(r.TeamId, ct))
            return Result<MediaResponse>.Failure(Forbidden);
        try
        {
            var now = clock.UtcNow;
            var item = MediaItem.Create(Guid.NewGuid(), r.TeamId, MediaSourceType.EXTERNAL_REFERENCE, r.Category, r.Title!, r.Description, a.UserId!.Value, now);
            var source = ExternalMediaReference.Create(Guid.NewGuid(), item.Id, r.Url!, r.ProviderLabel);
            db.MediaItems.Add(item);
            db.ExternalMediaReferences.Add(source);
            audit.Add(AuditPayload.Create(a.UserId.Value, AuditActions.MediaItemCreated, AuditEntityTypes.MediaItem, item.Id, now, null, new
            {
                teamId = item.TeamId,
                sourceType = item.SourceType,
                category = item.Category,
                title = item.Title,
                providerLabel = source.ProviderLabel
            }));
            await db.SaveChangesAsync(ct);
            return Result<MediaResponse>.Success((await ReadAsync(item.Id, ct))!);
        }
        catch (ArgumentException)
        {
            return Result<MediaResponse>.Failure(Validation);
        }
    }
    public async Task<Result<MediaResponse>> UpdateAsync(Guid id, UpdateMediaRequest r, CancellationToken ct)
    {
        var item = await db.MediaItems.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item is null)
            return Result<MediaResponse>.Failure(NotFound);
        var a = await current.GetAsync(ct);
        if (!CanMutate(a) || !await teamAccess.CanAccessAsync(item.TeamId, ct))
            return Result<MediaResponse>.Failure(Forbidden);
        try
        {
            var changed = item.Update(r.Category, r.Title!, r.Description, clock.UtcNow);
            if (item.SourceType == MediaSourceType.EXTERNAL_REFERENCE)
            { var ext = await db.ExternalMediaReferences.SingleAsync(x => x.MediaItemId == id, ct); changed |= ext.Update(r.Url!, r.ProviderLabel); }
            if (!changed)
                return Result<MediaResponse>.Success((await ReadAsync(id, ct))!);
            audit.Add(AuditPayload.Create(a.UserId!.Value, AuditActions.MediaItemUpdated, AuditEntityTypes.MediaItem, id, clock.UtcNow, null, new { category = item.Category, title = item.Title }));
            await db.SaveChangesAsync(ct);
            return Result<MediaResponse>.Success((await ReadAsync(id, ct))!);
        }
        catch (InvalidOperationException)
        {
            return Result<MediaResponse>.Failure(Conflict);
        }
        catch (ArgumentException)
        {
            return Result<MediaResponse>.Failure(Validation);
        }
    }
    public Task<Result<MediaResponse>> ArchiveAsync(Guid id, CancellationToken ct) => ChangeAsync(id, true, ct); public Task<Result<MediaResponse>> RestoreAsync(Guid id, CancellationToken ct) => ChangeAsync(id, false, ct);
    private async Task<Result<MediaResponse>> ChangeAsync(Guid id, bool archive, CancellationToken ct)
    {
        var a = await current.GetAsync(ct);
        if (!a.IsAdmin)
            return Result<MediaResponse>.Failure(Forbidden);
        var i = await db.MediaItems.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (i is null)
            return Result<MediaResponse>.Failure(NotFound);
        try
        {
            if (archive)
                i.Archive(a.UserId!.Value, clock.UtcNow);
            else
                i.Restore(a.UserId!.Value, clock.UtcNow);
            audit.Add(AuditPayload.Create(a.UserId.Value, archive ? AuditActions.MediaItemArchived : AuditActions.MediaItemRestored, AuditEntityTypes.MediaItem, id, clock.UtcNow, null, new
            {
                isArchived = archive
            }));
            await db.SaveChangesAsync(ct);
            return Result<MediaResponse>.Success((await ReadAsync(id, ct))!);
        }
        catch (InvalidOperationException)
        {
            return Result<MediaResponse>.Failure(Conflict);
        }
    }
    private async Task<bool> CanRead(MediaItem i, CancellationToken ct)
    {
        var a = await current.GetAsync(ct);
        return a.IsActive && a.HasAccessProfile && (!i.IsArchived || a.IsAdmin) && (a.IsAdmin || await teamAccess.CanAccessAsync(i.TeamId, ct));
    }
    private static bool CanMutate(CurrentUserAccess a) => a.IsAdmin || a.IsActive && a.PrimaryRole == Domain.Staff.StaffRole.DATA_OPERATOR;
    private async Task<MediaResponse?> ReadAsync(Guid id, CancellationToken ct)
    {
        var row = await (from m in db.MediaItems.AsNoTracking()
                         join t in db.Teams.AsNoTracking() on m.TeamId equals t.Id
                         join asset0 in db.MediaAssets.AsNoTracking() on m.Id equals asset0.MediaItemId into assets
                         from asset in assets.DefaultIfEmpty()
                         join f0 in db.StoredFiles.AsNoTracking() on asset.StoredFileId equals f0.Id into files
                         from f in files.DefaultIfEmpty()
                         join ext0 in db.ExternalMediaReferences.AsNoTracking() on m.Id equals ext0.MediaItemId into exts
                         from ext in exts.DefaultIfEmpty()
                         where m.Id == id
                         select new
                         {
                             m,
                             t,
                             asset,
                             f,
                             ext
                         }).SingleOrDefaultAsync(ct);
        if (row is null)
            return null;
        return new(row.m.Id, row.m.TeamId, row.t.Name, row.m.SourceType, row.m.Category, row.m.Title, row.m.Description, new(row.m.SourceType, row.f == null ? null : row.f.OriginalFileName, row.f == null ? null : row.f.ContentType, row.f == null ? null : row.f.SizeBytes, row.ext == null ? null : row.ext.Url, row.ext == null ? null : row.ext.ProviderLabel), row.m.CreatedByUserId, row.m.CreatedAtUtc, row.m.UpdatedAtUtc, row.m.IsArchived, row.m.ArchivedAtUtc);
    }
}
