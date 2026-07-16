using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Application.Auditing;
using PlayerPerformance.Application.Authorization;
using PlayerPerformance.Application.Files;
using PlayerPerformance.Application.Imports;
using PlayerPerformance.Domain.Auditing;
using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Files;
using PlayerPerformance.Domain.Imports;
using PlayerPerformance.Domain.Staff;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.Infrastructure.Imports;

internal sealed class ImportService(AppDbContext db, ICurrentUserAccess current, ITeamAccessService teamAccess, ISystemClock clock, IAuditWriter audit, IFileStorage storage, IFileStorageKeyGenerator keys, IOptions<ImportOptions> options, IImportProcessorRegistry processors) : IImportService
{
    private static readonly Error Forbidden = new("forbidden", "You do not have access to import data."), NotFound = new("not_found", "The requested import is not available."), Validation = new("validation_failed", "The import metadata or source file is invalid."), Conflict = new("import_conflict", "The requested import operation is not available."), Unsupported = new("unsupported_processing", "No processor is registered for this import.");
    public async Task<Result<ImportCapabilitiesResponse>> GetCapabilitiesAsync(CancellationToken ct) => await CanUseAsync(ct) ? Result<ImportCapabilitiesResponse>.Success(new(options.Value.MaxUploadSizeBytes, options.Value.PreviewRowLimit, options.Value.ProcessingLeaseTimeoutMinutes, ImportUploadRules.Capabilities, Enum.GetValues<ImportType>().OrderBy(x => x).ToList(), Enum.GetValues<ImportSourceSystem>().OrderBy(x => x).ToList(), processors.Capabilities)) : Result<ImportCapabilitiesResponse>.Failure(Forbidden);
    public async Task<Result<ImportJobResponse>> CreateAsync(CreateImportRequest request, CancellationToken ct)
    {
        var access = await current.GetAsync(ct);
        if (!CanUse(access) || !await teamAccess.CanAccessAsync(request.TeamId, ct))
            return Result<ImportJobResponse>.Failure(Forbidden);
        if (!ImportUploadRules.TryGetFormat(request.OriginalFileName, request.ContentType, out var format) || request.Content is null || request.DeclaredLength is <= 0 || request.DeclaredLength > options.Value.MaxUploadSizeBytes || !await ValidTargetAsync(request.TeamId, request.MatchId, request.ImportType, ct))
            return Result<ImportJobResponse>.Failure(Validation);
        var name = FileMetadataValidation.NormalizeOriginalFileName(request.OriginalFileName);
        if (name.IsFailure)
            return Result<ImportJobResponse>.Failure(Validation);
        var key = keys.Create();
        var write = await storage.WriteAsync(new(request.Content, key, request.DeclaredLength), ct);
        if (write.IsFailure || write.Value.SizeBytes > options.Value.MaxUploadSizeBytes)
        {
            if (write.IsSuccess)
                await FileStorageCompensation.DeleteAfterPersistenceFailureAsync(storage, key, ct);
            return Result<ImportJobResponse>.Failure(write.IsFailure ? write.Error : Validation);
        }
        try
        {
            var now = clock.UtcNow;
            var file = StoredFile.Create(Guid.NewGuid(), key, name.Value, request.ContentType!.Trim(), write.Value.SizeBytes, access.UserId!.Value, now);
            var job = ImportJob.Create(Guid.NewGuid(), request.TeamId, request.MatchId, file.Id, request.ImportType, request.SourceSystem, request.SourceLabel, format, request.Description, access.UserId.Value, now);
            db.StoredFiles.Add(file);
            db.ImportJobs.Add(job);
            audit.Add(AuditPayload.Create(access.UserId.Value, AuditActions.ImportJobCreated, AuditEntityTypes.ImportJob, job.Id, now, null, new { job.TeamId, job.MatchId, job.ImportType, job.SourceSystem, job.FileFormat, originalFileName = file.OriginalFileName, file.ContentType, file.SizeBytes }));
            await db.SaveChangesAsync(ct);
            return Result<ImportJobResponse>.Success(ToResponse(job, file));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or DbUpdateException)
        {
            await FileStorageCompensation.DeleteAfterPersistenceFailureAsync(storage, key, ct);
            return Result<ImportJobResponse>.Failure(Conflict);
        }
    }
    public async Task<Result<PagedImportJobsResponse>> ListAsync(ImportListQuery q, CancellationToken ct)
    {
        if (!await CanUseAsync(ct) || q.Page < 1 || q.PageSize is < 1 or > 100 || q.Search?.Length > 200 || q.DateFrom > q.DateTo)
            return Result<PagedImportJobsResponse>.Failure(Forbidden);
        if (q.TeamId.HasValue && !await teamAccess.CanAccessAsync(q.TeamId.Value, ct))
            return Result<PagedImportJobsResponse>.Failure(Forbidden);
        var a = await current.GetAsync(ct);
        var source = db.ImportJobs.AsNoTracking().Where(x => a.IsAdmin || a.TeamScopeType == TeamScopeType.ALL_TEAMS || a.SelectedTeamIds.Contains(x.TeamId));
        if (q.TeamId.HasValue)
            source = source.Where(x => x.TeamId == q.TeamId);
        if (q.MatchId.HasValue)
            source = source.Where(x => x.MatchId == q.MatchId);
        if (q.ImportType.HasValue)
            source = source.Where(x => x.ImportType == q.ImportType);
        if (q.SourceSystem.HasValue)
            source = source.Where(x => x.SourceSystem == q.SourceSystem);
        if (q.FileFormat.HasValue)
            source = source.Where(x => x.FileFormat == q.FileFormat);
        if (q.Status.HasValue)
            source = source.Where(x => x.Status == q.Status);
        if (q.DateFrom.HasValue)
            source = source.Where(x => x.CreatedAtUtc >= q.DateFrom);
        if (q.DateTo.HasValue)
            source = source.Where(x => x.CreatedAtUtc <= q.DateTo);
        if (!string.IsNullOrWhiteSpace(q.Search))
        { var term = q.Search.Trim().ToUpper(); source = source.Where(x => (x.Description != null && x.Description.ToUpper().Contains(term)) || (x.SourceLabel != null && x.SourceLabel.ToUpper().Contains(term))); }
        var total = await source.CountAsync(ct);
        var rows = await (from job in source.OrderByDescending(x => x.CreatedAtUtc).ThenBy(x => x.Id).Skip((q.Page - 1) * q.PageSize).Take(q.PageSize) join file in db.StoredFiles.AsNoTracking() on job.StoredFileId equals file.Id select new { job, file }).ToListAsync(ct);
        return Result<PagedImportJobsResponse>.Success(new(rows.Select(x => ToResponse(x.job, x.file)).ToList(), q.Page, q.PageSize, total, (int)Math.Ceiling(total / (double)q.PageSize)));
    }
    public async Task<ImportJobResponse?> GetAsync(Guid id, CancellationToken ct)
    {
        var row = await FindReadableAsync(id, ct);
        return row is null ? null : ToResponse(row.Value.job, row.Value.file);
    }
    public async Task<Result<ImportSourceResponse>> OpenSourceAsync(Guid id, CancellationToken ct)
    {
        var row = await FindReadableAsync(id, ct);
        if (row is null)
            return Result<ImportSourceResponse>.Failure(NotFound);
        var stream = await storage.OpenReadAsync(row.Value.file.StorageKey, ct);
        return stream.IsSuccess ? Result<ImportSourceResponse>.Success(new(stream.Value, row.Value.file.ContentType, row.Value.file.OriginalFileName)) : Result<ImportSourceResponse>.Failure(stream.Error);
    }

    public async Task<Result<ImportPreviewResponse>> GetPreviewAsync(Guid id, int page, int pageSize, CancellationToken ct)
    {
        if (page < 1 || pageSize is < 1 or > 100 || await FindReadableAsync(id, ct) is null)
            return Result<ImportPreviewResponse>.Failure(NotFound);
        var cols = await db.ImportPreviewColumns.AsNoTracking().Where(x => x.ImportJobId == id).OrderBy(x => x.Ordinal).Select(x => new ImportPreviewColumnResponse(x.Ordinal, x.SourceHeader, x.NormalizedHeader, x.DetectedDataType)).ToListAsync(ct);
        var all = db.ImportPreviewRows.AsNoTracking().Where(x => x.ImportJobId == id);
        var total = await all.CountAsync(ct);
        var rows = await all.OrderBy(x => x.SourceRowNumber).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Result<ImportPreviewResponse>.Success(new(cols, rows.Select(x => new ImportPreviewRowResponse(x.SourceRowNumber, JsonDocument.Parse(x.ValuesJson).RootElement.Clone())).ToList(), page, pageSize, total));
    }
    public async Task<Result<PagedImportValidationIssuesResponse>> GetIssuesAsync(Guid id, ImportValidationSeverity? severity, int page, int pageSize, CancellationToken ct)
    {
        if (page < 1 || pageSize is < 1 or > 100 || await FindReadableAsync(id, ct) is null)
            return Result<PagedImportValidationIssuesResponse>.Failure(NotFound);
        var source = db.ImportValidationIssues.AsNoTracking().Where(x => x.ImportJobId == id);
        if (severity.HasValue)
            source = source.Where(x => x.Severity == severity);
        var total = await source.CountAsync(ct);
        var issues = await source.OrderBy(x => x.SourceRowNumber).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Result<PagedImportValidationIssuesResponse>.Success(new(issues.Select(x => new ImportValidationIssueResponse(x.Severity, x.Code, x.Message, x.SourceRowNumber, x.ColumnKey, JsonDocument.Parse(x.MetadataJson).RootElement.Clone(), x.CreatedAtUtc)).ToList(), page, pageSize, total, (int)Math.Ceiling(total / (double)pageSize)));
    }
    public async Task<Result> ExecuteAsync(Guid id, ImportProcessingOperation operation, CancellationToken ct)
    {
        var job = await db.ImportJobs.SingleOrDefaultAsync(x => x.Id == id, ct);
        var access = await current.GetAsync(ct);
        if (job is null || !CanUse(access) || !await teamAccess.CanAccessAsync(job.TeamId, ct))
            return Result.Failure(NotFound);
        var processor = processors.Find(job.ImportType, job.SourceSystem, job.FileFormat);
        if (processor is null || operation switch
        {
            ImportProcessingOperation.PREVIEW => !processor.Capability.CanPreview,
            ImportProcessingOperation.VALIDATION => !processor.Capability.CanValidate,
            _ => !processor.Capability.CanConfirm
        })
            return Result.Failure(Unsupported);

        Guid lease;
        try
        {
            lease = job.AcquireLease(operation, processor.Capability.ProcessorKey, processor.Capability.ProcessorVersion, access.UserId!.Value, clock.UtcNow, TimeSpan.FromMinutes(options.Value.ProcessingLeaseTimeoutMinutes));
            await db.SaveChangesAsync(ct);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(Conflict);
        }

        try
        {
            var file = await db.StoredFiles.AsNoTracking().SingleAsync(x => x.Id == job.StoredFileId, ct);
            var source = await storage.OpenReadAsync(file.StorageKey, ct);
            if (source.IsFailure)
                throw new InvalidOperationException("The retained import source is unavailable.");
            await using var stream = source.Value;
            var context = new ImportProcessorContext(job.Id, job.TeamId, job.MatchId, job.ImportType, job.SourceSystem, job.FileFormat, job.ConfigurationRevision);
            if (operation == ImportProcessingOperation.PREVIEW)
            {
                var preview = await processor.GeneratePreviewAsync(context, stream, ct);
                if (preview.Rows.Count > options.Value.PreviewRowLimit)
                    throw new InvalidOperationException("The preview result exceeds the configured row limit.");
                var final = await db.ImportJobs.SingleAsync(x => x.Id == id, ct);
                if (!final.IsLeaseCurrent(lease))
                    return Result.Failure(Conflict);
                db.ImportPreviewColumns.RemoveRange(db.ImportPreviewColumns.Where(x => x.ImportJobId == id));
                db.ImportPreviewRows.RemoveRange(db.ImportPreviewRows.Where(x => x.ImportJobId == id));
                db.ImportPreviewColumns.AddRange(preview.Columns.Select(x => Domain.Imports.ImportPreviewColumn.Create(Guid.NewGuid(), id, x.Ordinal, x.SourceHeader, x.NormalizedHeader, x.DetectedDataType)));
                db.ImportPreviewRows.AddRange(preview.Rows.Select(x => Domain.Imports.ImportPreviewRow.Create(Guid.NewGuid(), id, x.SourceRowNumber, x.ValuesJson)));
                final.CompletePreview(lease, preview.Rows.Count, clock.UtcNow);
                audit.Add(AuditPayload.Create(access.UserId.Value, AuditActions.ImportJobPreviewed, AuditEntityTypes.ImportJob, id, clock.UtcNow, null, new
                {
                    previewRowCount = preview.Rows.Count,
                    columnCount = preview.Columns.Count
                }));
                await db.SaveChangesAsync(ct);
                return Result.Success();
            }
            if (operation == ImportProcessingOperation.VALIDATION)
            {
                var validation = await processor.ValidateAsync(context, stream, ct);
                var final = await db.ImportJobs.SingleAsync(x => x.Id == id, ct);
                if (!final.IsLeaseCurrent(lease))
                    return Result.Failure(Conflict);
                db.ImportValidationIssues.RemoveRange(db.ImportValidationIssues.Where(x => x.ImportJobId == id));
                db.ImportValidationIssues.AddRange(validation.Issues.Select(x => Domain.Imports.ImportValidationIssue.Create(Guid.NewGuid(), id, x.Severity, x.Code, x.Message, x.SourceRowNumber, x.ColumnKey, x.MetadataJson, clock.UtcNow)));
                var errors = validation.Issues.Any(x => x.Severity == ImportValidationSeverity.ERROR);
                final.CompleteValidation(lease, processor.Capability.ProcessorKey, processor.Capability.ProcessorVersion, errors, validation.TotalRowCount, validation.ValidRowCount, validation.InvalidRowCount, validation.WarningCount, clock.UtcNow);
                audit.Add(AuditPayload.Create(access.UserId.Value, AuditActions.ImportJobValidated, AuditEntityTypes.ImportJob, id, clock.UtcNow, null, new { hasErrors = errors, validation.WarningCount, issueCount = validation.Issues.Count }));
                await db.SaveChangesAsync(ct);
                return Result.Success();
            }
            var confirmation = await processor.ConfirmAsync(context, stream, ct);
            var confirmationFinal = await db.ImportJobs.SingleAsync(x => x.Id == id, ct);
            if (!confirmationFinal.IsLeaseCurrent(lease))
                return Result.Failure(Conflict);
            confirmationFinal.CompleteConfirmation(lease, access.UserId.Value, confirmation.ResultSummaryJson, clock.UtcNow);
            audit.Add(AuditPayload.Create(access.UserId.Value, AuditActions.ImportJobConfirmed, AuditEntityTypes.ImportJob, id, clock.UtcNow));
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            var final = await db.ImportJobs.SingleOrDefaultAsync(x => x.Id == id, ct);
            if (final?.IsLeaseCurrent(lease) == true)
            {
                final.Fail(lease, "PROCESSING_FAILED", "The import processor could not complete the requested operation.", clock.UtcNow);
                audit.Add(AuditPayload.Create(access.UserId!.Value, AuditActions.ImportJobFailed, AuditEntityTypes.ImportJob, id, clock.UtcNow, null, new { operation }));
                await db.SaveChangesAsync(ct);
            }
            return Result.Failure(Conflict);
        }
    }
    public async Task<Result> CancelAsync(Guid id, CancellationToken ct)
    {
        var job = await db.ImportJobs.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (job is null)
            return Result.Failure(NotFound);
        var access = await current.GetAsync(ct);
        if (!CanUse(access) || !await teamAccess.CanAccessAsync(job.TeamId, ct))
            return Result.Failure(NotFound);
        try
        {
            var now = clock.UtcNow;
            job.Cancel(access.UserId!.Value, now, TimeSpan.FromMinutes(options.Value.ProcessingLeaseTimeoutMinutes));
            audit.Add(AuditPayload.Create(access.UserId.Value, AuditActions.ImportJobCancelled, AuditEntityTypes.ImportJob, id, now));
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(Conflict);
        }
    }
    private async Task<bool> CanUseAsync(CancellationToken ct) => CanUse(await current.GetAsync(ct));
    private static bool CanUse(CurrentUserAccess access) => access.IsAdmin || access.IsActive && access.PrimaryRole == StaffRole.DATA_OPERATOR && access.EffectivePermissions.CanImportData;
    private async Task<bool> ValidTargetAsync(Guid teamId, Guid? matchId, ImportType type, CancellationToken ct)
    {
        var team = await db.Teams.AsNoTracking().SingleOrDefaultAsync(x => x.Id == teamId, ct);
        if (team is null || team.Status == TeamStatus.ARCHIVED)
            return false;
        if (type is ImportType.MATCH_GPS or ImportType.MATCH_PLAYER_STATISTICS)
            return matchId.HasValue && await db.Matches.AsNoTracking().AnyAsync(x => x.Id == matchId && x.TeamId == teamId && !x.IsArchived, ct);
        return !matchId.HasValue;
    }
    private async Task<(ImportJob job, StoredFile file)?> FindReadableAsync(Guid id, CancellationToken ct)
    {
        var row = await (from job in db.ImportJobs.AsNoTracking() join file in db.StoredFiles.AsNoTracking() on job.StoredFileId equals file.Id where job.Id == id select new { job, file }).SingleOrDefaultAsync(ct);
        return row is null || !await CanUseAsync(ct) || !await teamAccess.CanAccessAsync(row.job.TeamId, ct) ? null : (row.job, row.file);
    }
    private static ImportJobResponse ToResponse(ImportJob job, StoredFile file) => new(job.Id, job.TeamId, job.MatchId, job.ImportType, job.SourceSystem, job.SourceLabel, job.FileFormat, job.Status, job.Description, file.OriginalFileName, file.ContentType, file.SizeBytes, job.CreatedAtUtc, job.UpdatedAtUtc, job.ConfigurationRevision, job.ValidatedConfigurationRevision, job.ValidatedAtUtc, job.PreviewGeneratedAtUtc, job.ValidationCompletedAtUtc, job.TotalRowCount, job.PreviewRowCount, job.ValidRowCount, job.InvalidRowCount, job.WarningCount, job.FailureCode, job.FailureMessage, job.ConfirmedAtUtc, job.CancelledAtUtc, job.Status is ImportJobStatus.UPLOADED or ImportJobStatus.VALIDATION_FAILED or ImportJobStatus.FAILED or ImportJobStatus.READY_TO_CONFIRM ? [ImportAllowedAction.CANCEL] : []);
}
