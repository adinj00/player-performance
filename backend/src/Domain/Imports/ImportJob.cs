using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;

namespace PlayerPerformance.Domain.Imports;

public sealed class ImportJob : Entity
{
    public const int SourceLabelMaxLength = 100, DescriptionMaxLength = 1000, FailureCodeMaxLength = 100, FailureMessageMaxLength = 1000, ProcessorKeyMaxLength = 200, ProcessorVersionMaxLength = 100;
    private ImportJob() : base(Guid.Empty)
    {
        ResultSummaryJson = "{}";
        PreviewMetadataJson = "{}";
    }
    private ImportJob(Guid id, Guid teamId, Guid? matchId, Guid? trainingSessionId, Guid storedFileId, ImportType type, ImportSourceSystem source, string? label, ImportFileFormat format, string? description, Guid userId, DateTime now) : base(id)
    {
        TeamId = teamId;
        MatchId = matchId;
        TrainingSessionId = trainingSessionId;
        StoredFileId = storedFileId;
        ImportType = type;
        SourceSystem = source;
        SourceLabel = label;
        FileFormat = format;
        Description = description;
        CreatedByUserId = userId;
        CreatedAtUtc = UpdatedAtUtc = now;
        Status = ImportJobStatus.UPLOADED;
        ConfigurationRevision = 1;
        ResultSummaryJson = "{}";
        PreviewMetadataJson = "{}";
    }
    public Guid TeamId { get; private set; }
    public Guid? MatchId { get; private set; }
    public Guid? TrainingSessionId { get; private set; }
    public Guid StoredFileId { get; private set; }
    public ImportType ImportType { get; private set; }
    public ImportSourceSystem SourceSystem { get; private set; }
    public string? SourceLabel { get; private set; }
    public ImportFileFormat FileFormat { get; private set; }
    public ImportJobStatus Status { get; private set; }
    public string? Description { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public int ConfigurationRevision { get; private set; }
    public int? ValidatedConfigurationRevision { get; private set; }
    public string? ValidatedProcessorKey { get; private set; }
    public string? ValidatedProcessorVersion { get; private set; }
    public DateTime? ValidatedAtUtc { get; private set; }
    public DateTime? PreviewGeneratedAtUtc { get; private set; }
    public DateTime? ValidationCompletedAtUtc { get; private set; }
    public int? TotalRowCount { get; private set; }
    public int? PreviewRowCount { get; private set; }
    public int? ValidRowCount { get; private set; }
    public int? InvalidRowCount { get; private set; }
    public int? WarningCount { get; private set; }
    public string? FailureCode { get; private set; }
    public string? FailureMessage { get; private set; }
    public string ResultSummaryJson { get; private set; }
    public string PreviewMetadataJson { get; private set; }
    public ImportProcessingOperation? ProcessingOperation { get; private set; }
    public Guid? ProcessingLeaseId { get; private set; }
    public DateTime? ProcessingStartedAtUtc { get; private set; }
    public Guid? ProcessingRequestedByUserId { get; private set; }
    public string? ProcessingProcessorKey { get; private set; }
    public string? ProcessingProcessorVersion { get; private set; }
    public Guid? ConfirmedByUserId { get; private set; }
    public DateTime? ConfirmedAtUtc { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public static ImportJob Create(Guid id, Guid teamId, Guid? matchId, Guid storedFileId, ImportType type, ImportSourceSystem source, string? sourceLabel, ImportFileFormat format, string? description, Guid userId, DateTime now)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(teamId, nameof(teamId));
        Guard.AgainstDefault(storedFileId, nameof(storedFileId));
        Guard.AgainstDefault(userId, nameof(userId));
        EnsureTarget(type, matchId, null);
        if (!Enum.IsDefined(source) || !Enum.IsDefined(format))
            throw new ArgumentOutOfRangeException();
        var label = Normalize(sourceLabel, SourceLabelMaxLength);
        if ((source == ImportSourceSystem.OTHER) != (label is not null))
            throw new ArgumentException("A source label is required only for OTHER.");
        return new(id, teamId, matchId, null, storedFileId, type, source, label, format, Normalize(description, DescriptionMaxLength), userId, now);
    }
    public static ImportJob Create(Guid id, Guid teamId, Guid? matchId, Guid? trainingSessionId, Guid storedFileId, ImportType type, ImportSourceSystem source, string? sourceLabel, ImportFileFormat format, string? description, Guid userId, DateTime now)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(teamId, nameof(teamId));
        Guard.AgainstDefault(storedFileId, nameof(storedFileId));
        Guard.AgainstDefault(userId, nameof(userId));
        EnsureTarget(type, matchId, trainingSessionId);
        var label = Normalize(sourceLabel, SourceLabelMaxLength);
        if ((source == ImportSourceSystem.OTHER) != (label is not null))
            throw new ArgumentException("A source label is required only for OTHER.");
        return new(id, teamId, matchId, trainingSessionId, storedFileId, type, source, label, format, Normalize(description, DescriptionMaxLength), userId, now);
    }
    public Guid AcquireLease(ImportProcessingOperation operation, string key, string version, Guid actorId, DateTime now, TimeSpan timeout)
    {
        var staleLease = Status == ImportJobStatus.PARSING && ProcessingStartedAtUtc <= now - timeout;
        if (Status == ImportJobStatus.PARSING && !staleLease)
            throw new InvalidOperationException("An active processing lease exists.");
        if (Status is ImportJobStatus.IMPORTED or ImportJobStatus.CANCELLED || !staleLease && !CanProcessFrom(Status) || operation == ImportProcessingOperation.CONFIRMATION && Status != ImportJobStatus.READY_TO_CONFIRM)
            throw new InvalidOperationException("The requested processing transition is invalid.");
        Status = ImportJobStatus.PARSING;
        ProcessingOperation = operation;
        ProcessingLeaseId = Guid.NewGuid();
        ProcessingStartedAtUtc = now;
        ProcessingRequestedByUserId = actorId;
        ProcessingProcessorKey = Ensure(key, ProcessorKeyMaxLength);
        ProcessingProcessorVersion = Ensure(version, ProcessorVersionMaxLength);
        UpdatedAtUtc = now;
        return ProcessingLeaseId.Value;
    }
    public bool IsLeaseCurrent(Guid leaseId) => Status == ImportJobStatus.PARSING && ProcessingLeaseId == leaseId;
    public void CompletePreview(Guid leaseId, int previewRowCount, int totalRowCount, string metadataJson, DateTime now)
    {
        EnsureLease(leaseId);
        Status = ImportJobStatus.UPLOADED;
        PreviewGeneratedAtUtc = now;
        PreviewRowCount = previewRowCount;
        TotalRowCount = totalRowCount;
        PreviewMetadataJson = metadataJson;
        FailureCode = null;
        FailureMessage = null;
        ClearLease();
        UpdatedAtUtc = now;
    }
    public void CompletePreview(Guid leaseId, int previewRowCount, DateTime now) => CompletePreview(leaseId, previewRowCount, previewRowCount, "{}", now);
    public void CompleteValidation(Guid leaseId, string key, string version, bool hasErrors, int? totalRows, int? validRows, int? invalidRows, int? warningCount, DateTime now)
    {
        EnsureLease(leaseId);
        Status = hasErrors ? ImportJobStatus.VALIDATION_FAILED : ImportJobStatus.READY_TO_CONFIRM;
        ValidationCompletedAtUtc = now;
        TotalRowCount = totalRows;
        ValidRowCount = validRows;
        InvalidRowCount = invalidRows;
        WarningCount = warningCount;
        ValidatedConfigurationRevision = hasErrors ? null : ConfigurationRevision;
        ValidatedProcessorKey = hasErrors ? null : Ensure(key, ProcessorKeyMaxLength);
        ValidatedProcessorVersion = hasErrors ? null : Ensure(version, ProcessorVersionMaxLength);
        ValidatedAtUtc = hasErrors ? null : now;
        ClearLease();
        UpdatedAtUtc = now;
    }
    public void CompleteConfirmation(Guid leaseId, Guid actorId, string summaryJson, DateTime now)
    {
        EnsureLease(leaseId);
        if (ValidatedConfigurationRevision != ConfigurationRevision || ValidatedProcessorKey != ProcessingProcessorKey || ValidatedProcessorVersion != ProcessingProcessorVersion)
            throw new InvalidOperationException("Validation is stale.");
        Status = ImportJobStatus.IMPORTED;
        ConfirmedByUserId = actorId;
        ConfirmedAtUtc = now;
        ResultSummaryJson = summaryJson;
        ClearLease();
        UpdatedAtUtc = now;
    }
    public void Fail(Guid leaseId, string code, string message, DateTime now)
    {
        EnsureLease(leaseId);
        Status = ImportJobStatus.FAILED;
        FailureCode = Ensure(code, FailureCodeMaxLength);
        FailureMessage = Ensure(message, FailureMessageMaxLength);
        ClearLease();
        UpdatedAtUtc = now;
    }
    public void Cancel(Guid actorId, DateTime now, TimeSpan timeout)
    {
        if (Status == ImportJobStatus.PARSING && ProcessingStartedAtUtc > now - timeout)
            throw new InvalidOperationException("An active processing lease exists.");
        if (Status is ImportJobStatus.IMPORTED or ImportJobStatus.CANCELLED)
            throw new InvalidOperationException("Terminal import jobs cannot be cancelled.");
        if (Status == ImportJobStatus.PARSING)
            ClearLease();
        Status = ImportJobStatus.CANCELLED;
        CancelledByUserId = actorId;
        CancelledAtUtc = now;
        UpdatedAtUtc = now;
    }
    private void EnsureLease(Guid leaseId)
    {
        if (!IsLeaseCurrent(leaseId))
            throw new InvalidOperationException("The processing lease is stale.");
    }
    private void ClearLease()
    {
        ProcessingOperation = null;
        ProcessingLeaseId = null;
        ProcessingStartedAtUtc = null;
        ProcessingRequestedByUserId = null;
        ProcessingProcessorKey = null;
        ProcessingProcessorVersion = null;
    }
    private static bool CanProcessFrom(ImportJobStatus status) => status is ImportJobStatus.UPLOADED or ImportJobStatus.VALIDATION_FAILED or ImportJobStatus.FAILED or ImportJobStatus.READY_TO_CONFIRM;
    private static void EnsureTarget(ImportType type, Guid? matchId, Guid? trainingSessionId)
    {
        if (!Enum.IsDefined(type) || matchId.HasValue && trainingSessionId.HasValue || ((type is ImportType.MATCH_GPS or ImportType.MATCH_PLAYER_STATISTICS) != matchId.HasValue) || trainingSessionId.HasValue && type != ImportType.TRAINING_GPS)
            throw new ArgumentException("The import type and target context do not match.");
    }
    private static string? Normalize(string? value, int limit) => string.IsNullOrWhiteSpace(value) ? null : Ensure(value.Trim(), limit);
    private static string Ensure(string value, int limit)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > limit || value.Any(char.IsControl))
            throw new ArgumentOutOfRangeException(nameof(value));
        return value.Trim();
    }
}
