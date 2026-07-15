using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Application.Auditing;
using PlayerPerformance.Application.Authorization;
using PlayerPerformance.Application.Media;
using PlayerPerformance.Application.Files;
using PlayerPerformance.Domain.Files;
using PlayerPerformance.Domain.Auditing;
using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Media;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Domain.Players;
using PlayerPerformance.Infrastructure.Persistence;
namespace PlayerPerformance.Infrastructure.Media;

internal sealed class MediaService(AppDbContext db, ICurrentUserAccess current, ITeamAccessService teamAccess, ISystemClock clock, IAuditWriter audit, IFileStorage storage, IFileStorageKeyGenerator keyGenerator, Microsoft.Extensions.Options.IOptions<MediaOptions> options, ILogger<MediaService> logger) : IMediaService
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
        if (q.LinkedTargetType.HasValue != q.LinkedTargetId.HasValue)
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
        if (q.LinkedTargetType.HasValue)
            s = q.LinkedTargetType.Value switch
            {
                MediaLinkTargetType.MATCH => s.Where(x => db.MediaMatchLinks.Any(link => link.MediaItemId == x.Id && link.MatchId == q.LinkedTargetId && link.UnlinkedAtUtc == null)),
                MediaLinkTargetType.MATCH_REPORT => s.Where(x => db.MediaMatchReportLinks.Any(link => link.MediaItemId == x.Id && link.MatchReportId == q.LinkedTargetId && link.UnlinkedAtUtc == null)),
                MediaLinkTargetType.PLAYER => s.Where(x => db.MediaPlayerLinks.Any(link => link.MediaItemId == x.Id && link.PlayerId == q.LinkedTargetId && link.UnlinkedAtUtc == null)),
                _ => s.Where(_ => false)
            };
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
    public async Task<Result<MediaResponse>> CreateAssetAsync(CreateMediaAssetRequest r, CancellationToken ct)
    {
        var a = await current.GetAsync(ct);
        var canMutate = CanMutate(a);
        var canAccessTeam = await teamAccess.CanAccessAsync(r.TeamId, ct);
        if (!canMutate || !canAccessTeam)
        {
            logger.LogWarning(
                "Media asset upload denied for user {UserId}. Active: {IsActive}; access profile: {HasAccessProfile}; role: {PrimaryRole}; administrator: {IsAdmin}; team: {TeamId}; team access: {CanAccessTeam}.",
                a.UserId,
                a.IsActive,
                a.HasAccessProfile,
                a.PrimaryRole,
                a.IsAdmin,
                r.TeamId,
                canAccessTeam);
            return Result<MediaResponse>.Failure(!canMutate
                ? new Error("forbidden", "Only administrators and data operators can upload media.")
                : new Error("forbidden", "You do not have access to the selected team."));
        }
        var normalized = FileMetadataValidation.NormalizeOriginalFileName(r.OriginalFileName);
        if (normalized.IsFailure || r.Content is null || r.DeclaredLength is <= 0 || r.DeclaredLength > options.Value.MaxUploadSizeBytes || !FileMetadataValidation.IsValidContentType(r.ContentType) || !MediaUploadRules.IsAllowed(r.Category, normalized.IsSuccess ? normalized.Value : string.Empty, r.ContentType ?? string.Empty))
            return Result<MediaResponse>.Failure(Validation);
        var key = keyGenerator.Create();
        var write = await storage.WriteAsync(new(r.Content, key, r.DeclaredLength), ct);
        if (write.IsFailure || write.Value.SizeBytes > options.Value.MaxUploadSizeBytes)
            return Result<MediaResponse>.Failure(write.IsFailure ? write.Error : Validation);
        try
        {
            var now = clock.UtcNow;
            var stored = StoredFile.Create(Guid.NewGuid(), key, normalized.Value, r.ContentType!.Trim(), write.Value.SizeBytes, a.UserId!.Value, now);
            var item = MediaItem.Create(Guid.NewGuid(), r.TeamId, MediaSourceType.UPLOADED_FILE, r.Category, r.Title!, r.Description, a.UserId.Value, now);
            db.StoredFiles.Add(stored);
            db.MediaItems.Add(item);
            db.MediaAssets.Add(MediaAsset.Create(Guid.NewGuid(), item.Id, stored.Id));
            audit.Add(AuditPayload.Create(a.UserId.Value, AuditActions.MediaItemCreated, AuditEntityTypes.MediaItem, item.Id, now, null, new { teamId = item.TeamId, sourceType = item.SourceType, category = item.Category, title = item.Title, originalFileName = stored.OriginalFileName, contentType = stored.ContentType, sizeBytes = stored.SizeBytes }));
            await db.SaveChangesAsync(ct);
            return Result<MediaResponse>.Success((await ReadAsync(item.Id, ct))!);
        }
        catch (Exception exception) when (exception is DbUpdateException or InvalidOperationException or ArgumentException)
        {
            var compensation = await FileStorageCompensation.DeleteAfterPersistenceFailureAsync(storage, key, ct);
            if (compensation.IsFailure)
                logger.LogError("Media upload compensation failed for storage key {StorageKey}; correlation {TraceIdentifier}", key, System.Diagnostics.Activity.Current?.Id);
            return Result<MediaResponse>.Failure(Conflict);
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
    public async Task<Result<MediaContentResponse>> OpenContentAsync(Guid id, CancellationToken ct)
    {
        var item = await db.MediaItems.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item is null || !await CanRead(item, ct))
            return Result<MediaContentResponse>.Failure(NotFound);
        if (item.SourceType != MediaSourceType.UPLOADED_FILE)
            return Result<MediaContentResponse>.Failure(Conflict);
        var file = await (from asset in db.MediaAssets.AsNoTracking() join stored in db.StoredFiles.AsNoTracking() on asset.StoredFileId equals stored.Id where asset.MediaItemId == id select stored).SingleAsync(ct);
        var stream = await storage.OpenReadAsync(file.StorageKey, ct);
        return stream.IsSuccess ? Result<MediaContentResponse>.Success(new(stream.Value, file.ContentType, file.OriginalFileName)) : Result<MediaContentResponse>.Failure(stream.Error);
    }
    public async Task<Result> LinkAsync(Guid mediaId, MediaLinkTargetType type, Guid targetId, CancellationToken ct)
    {
        var item = await db.MediaItems.SingleOrDefaultAsync(x => x.Id == mediaId, ct);
        var access = await current.GetAsync(ct);
        if (item is null)
            return Result.Failure(NotFound);
        if (item.IsArchived || !CanMutate(access) || !await teamAccess.CanAccessAsync(item.TeamId, ct))
            return Result.Failure(Forbidden);
        var now = clock.UtcNow;
        switch (type)
        {
            case MediaLinkTargetType.MATCH:
                var match = await db.Matches.SingleOrDefaultAsync(x => x.Id == targetId, ct);
                if (match is null || match.TeamId != item.TeamId || match.IsArchived || match.Status == MatchStatus.CANCELLED || await db.MediaMatchLinks.AnyAsync(x => x.MediaItemId == mediaId && x.MatchId == targetId && x.UnlinkedAtUtc == null, ct))
                    return Result.Failure(Conflict);
                if (await db.MatchReports.AnyAsync(x => x.MatchId == targetId && (x.Status == MatchReportStatus.READY_FOR_REVIEW || x.Status == MatchReportStatus.VERIFIED || x.Status == MatchReportStatus.ARCHIVED), ct))
                    return Result.Failure(Conflict);
                db.MediaMatchLinks.Add(MediaMatchLink.Create(Guid.NewGuid(), mediaId, targetId, access.UserId!.Value, now));
                break;
            case MediaLinkTargetType.MATCH_REPORT:
                var report = await db.MatchReports.SingleOrDefaultAsync(x => x.Id == targetId, ct);
                if (report is null || report.Status is not (MatchReportStatus.DRAFT or MatchReportStatus.NEEDS_CORRECTION) || await db.MediaMatchReportLinks.AnyAsync(x => x.MediaItemId == mediaId && x.MatchReportId == targetId && x.UnlinkedAtUtc == null, ct))
                    return Result.Failure(Conflict);
                var reportMatch = await db.Matches.SingleAsync(x => x.Id == report.MatchId, ct);
                if (reportMatch.TeamId != item.TeamId)
                    return Result.Failure(Conflict);
                db.MediaMatchReportLinks.Add(MediaMatchReportLink.Create(Guid.NewGuid(), mediaId, targetId, access.UserId!.Value, now));
                break;
            case MediaLinkTargetType.PLAYER:
                var player = await db.Players.SingleOrDefaultAsync(x => x.Id == targetId, ct);
                if (player is null || player.Status == PlayerRecordStatus.ARCHIVED || !await db.PlayerTeamAssignments.AnyAsync(x => x.PlayerId == targetId && x.TeamId == item.TeamId, ct) || await db.MediaPlayerLinks.AnyAsync(x => x.MediaItemId == mediaId && x.PlayerId == targetId && x.UnlinkedAtUtc == null, ct))
                    return Result.Failure(Conflict);
                db.MediaPlayerLinks.Add(MediaPlayerLink.Create(Guid.NewGuid(), mediaId, targetId, access.UserId!.Value, now));
                break;
            default:
                return Result.Failure(Validation);
        }
        audit.Add(AuditPayload.Create(access.UserId!.Value, AuditActions.MediaItemLinked, AuditEntityTypes.MediaItem, mediaId, now, null, new
        {
            targetType = type,
            targetId
        }));
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
    public async Task<Result> UnlinkAsync(Guid mediaId, MediaLinkTargetType type, Guid targetId, CancellationToken ct)
    {
        var item = await db.MediaItems.SingleOrDefaultAsync(x => x.Id == mediaId, ct);
        var access = await current.GetAsync(ct);
        if (item is null)
            return Result.Failure(NotFound);
        if (item.IsArchived || !CanMutate(access) || !await teamAccess.CanAccessAsync(item.TeamId, ct))
            return Result.Failure(Forbidden);
        MediaLink? link = type switch { MediaLinkTargetType.MATCH => await db.MediaMatchLinks.SingleOrDefaultAsync(x => x.MediaItemId == mediaId && x.MatchId == targetId && x.UnlinkedAtUtc == null, ct), MediaLinkTargetType.MATCH_REPORT => await db.MediaMatchReportLinks.SingleOrDefaultAsync(x => x.MediaItemId == mediaId && x.MatchReportId == targetId && x.UnlinkedAtUtc == null, ct), MediaLinkTargetType.PLAYER => await db.MediaPlayerLinks.SingleOrDefaultAsync(x => x.MediaItemId == mediaId && x.PlayerId == targetId && x.UnlinkedAtUtc == null, ct), _ => null };
        if (link is null)
            return Result.Failure(Conflict);
        link.Unlink(access.UserId!.Value, clock.UtcNow);
        audit.Add(AuditPayload.Create(access.UserId.Value, AuditActions.MediaItemUnlinked, AuditEntityTypes.MediaItem, mediaId, clock.UtcNow, null, new { targetType = type, targetId }));
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
    public async Task<Result<PagedMediaLinkCandidatesResponse>> ListCandidatesAsync(Guid mediaId, MediaLinkTargetType type, string? search, int page, int pageSize, CancellationToken ct)
    {
        if (page < 1 || pageSize is < 1 or > 100 || search?.Length > 200)
            return Result<PagedMediaLinkCandidatesResponse>.Failure(Validation);
        var item = await db.MediaItems.AsNoTracking().SingleOrDefaultAsync(x => x.Id == mediaId, ct);
        var a = await current.GetAsync(ct);
        if (item is null)
            return Result<PagedMediaLinkCandidatesResponse>.Failure(NotFound);
        if (item.IsArchived || !CanMutate(a) || !await teamAccess.CanAccessAsync(item.TeamId, ct))
            return Result<PagedMediaLinkCandidatesResponse>.Failure(Forbidden);
        var term = search?.Trim().ToUpper();
        IReadOnlyList<MediaLinkCandidateResponse>? items = type switch
        {
            MediaLinkTargetType.MATCH => (await (from match in db.Matches.AsNoTracking()
                                                 join opponent in db.Opponents.AsNoTracking() on match.OpponentId equals opponent.Id
                                                 where match.TeamId == item.TeamId && !match.IsArchived && match.Status != MatchStatus.CANCELLED
                                                       && !db.MediaMatchLinks.Any(x => x.MediaItemId == mediaId && x.MatchId == match.Id && x.UnlinkedAtUtc == null)
                                                       && !db.MatchReports.Any(x => x.MatchId == match.Id && (x.Status == MatchReportStatus.READY_FOR_REVIEW || x.Status == MatchReportStatus.VERIFIED || x.Status == MatchReportStatus.ARCHIVED))
                                                       && (string.IsNullOrEmpty(term) || opponent.Name.ToUpper().Contains(term) || match.Round != null && match.Round.ToUpper().Contains(term))
                                                 orderby match.KickoffAtUtc descending
                                                 select new { match.Id, OpponentName = opponent.Name, match.KickoffAtUtc, match.Round, match.Status }).ToListAsync(ct))
                .Select(candidate => new MediaLinkCandidateResponse(candidate.Id, type, candidate.OpponentName, candidate.KickoffAtUtc.ToString("dd.MM.yyyy.") + (candidate.Round == null ? string.Empty : " · " + candidate.Round), candidate.Status.ToString()))
                .ToList(),
            MediaLinkTargetType.MATCH_REPORT => (await (from report in db.MatchReports.AsNoTracking()
                                                        join match in db.Matches.AsNoTracking() on report.MatchId equals match.Id
                                                        join opponent in db.Opponents.AsNoTracking() on match.OpponentId equals opponent.Id
                                                        where match.TeamId == item.TeamId && !match.IsArchived && (report.Status == MatchReportStatus.DRAFT || report.Status == MatchReportStatus.NEEDS_CORRECTION)
                                                              && !db.MediaMatchReportLinks.Any(x => x.MediaItemId == mediaId && x.MatchReportId == report.Id && x.UnlinkedAtUtc == null)
                                                              && (string.IsNullOrEmpty(term) || opponent.Name.ToUpper().Contains(term) || match.Round != null && match.Round.ToUpper().Contains(term))
                                                        orderby match.KickoffAtUtc descending
                                                        select new { report.Id, OpponentName = opponent.Name, match.KickoffAtUtc, match.Round, report.Status }).ToListAsync(ct))
                .Select(candidate => new MediaLinkCandidateResponse(candidate.Id, type, candidate.OpponentName, candidate.KickoffAtUtc.ToString("dd.MM.yyyy.") + (candidate.Round == null ? string.Empty : " · " + candidate.Round), candidate.Status.ToString()))
                .ToList(),
            MediaLinkTargetType.PLAYER => await (from player in db.Players.AsNoTracking()
                                                 where player.Status != PlayerRecordStatus.ARCHIVED && db.PlayerTeamAssignments.Any(x => x.PlayerId == player.Id && x.TeamId == item.TeamId)
                                                       && !db.MediaPlayerLinks.Any(x => x.MediaItemId == mediaId && x.PlayerId == player.Id && x.UnlinkedAtUtc == null)
                                                       && (string.IsNullOrEmpty(term) || player.FirstName.ToUpper().Contains(term) || player.LastName.ToUpper().Contains(term) || player.PreferredName != null && player.PreferredName.ToUpper().Contains(term))
                                                 orderby player.LastName, player.FirstName
                                                 select new MediaLinkCandidateResponse(player.Id, type, player.FirstName + " " + player.LastName, string.Empty, player.Status.ToString())).ToListAsync(ct),
            _ => null
        };
        if (items is null)
            return Result<PagedMediaLinkCandidatesResponse>.Failure(Validation);
        return Result<PagedMediaLinkCandidatesResponse>.Success(new(items.Skip((page - 1) * pageSize).Take(pageSize).ToList(), page, pageSize, items.Count, (int)Math.Ceiling(items.Count / (double)pageSize)));
    }
    private async Task<MediaResponse?> ReadAsync(Guid id, CancellationToken ct)
    {
        var item = await db.MediaItems.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, ct);
        if (item is null)
            return null;
        var teamName = await db.Teams.AsNoTracking().Where(team => team.Id == item.TeamId).Select(team => team.Name).SingleOrDefaultAsync(ct);
        if (teamName is null)
            return null;
        var asset = await db.MediaAssets.AsNoTracking().SingleOrDefaultAsync(value => value.MediaItemId == item.Id, ct);
        var stored = asset is null
            ? null
            : await db.StoredFiles.AsNoTracking().SingleOrDefaultAsync(value => value.Id == asset.StoredFileId, ct);
        var external = await db.ExternalMediaReferences.AsNoTracking().SingleOrDefaultAsync(value => value.MediaItemId == item.Id, ct);
        return new(item.Id, item.TeamId, teamName, item.SourceType, item.Category, item.Title, item.Description, new(item.SourceType, stored?.OriginalFileName, stored?.ContentType, stored?.SizeBytes, external?.Url, external?.ProviderLabel), item.CreatedByUserId, item.CreatedAtUtc, item.UpdatedAtUtc, item.IsArchived, item.ArchivedAtUtc);
    }
}
