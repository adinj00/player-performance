using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;

namespace PlayerPerformance.Domain.Auditing;

public sealed class AuditLog : Entity
{
    private AuditLog() : base(Guid.Empty) { }
    private AuditLog(Guid id, Guid actorUserId, string action, string entityType, Guid entityId, DateTime occurredAtUtc, string previousValuesJson, string newValuesJson, string metadataJson) : base(id)
    {
        ActorUserId = actorUserId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        OccurredAtUtc = occurredAtUtc;
        PreviousValuesJson = previousValuesJson;
        NewValuesJson = newValuesJson;
        MetadataJson = metadataJson;
    }
    public Guid ActorUserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public string PreviousValuesJson { get; private set; } = "{}";
    public string NewValuesJson { get; private set; } = "{}";
    public string MetadataJson { get; private set; } = "{}";
    public static AuditLog Create(Guid id, Guid actorUserId, string action, string entityType, Guid entityId, DateTime occurredAtUtc, string previousValuesJson, string newValuesJson, string metadataJson)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(actorUserId, nameof(actorUserId));
        Guard.AgainstDefault(entityId, nameof(entityId));
        if (!AuditActions.IsKnown(action) || !AuditEntityTypes.IsKnown(entityType))
            throw new ArgumentOutOfRangeException(nameof(action));
        return new(id, actorUserId, action, entityType, entityId, occurredAtUtc, previousValuesJson, newValuesJson, metadataJson);
    }
}
public static class AuditEntityTypes
{
    public const string StaffUser = "STAFF_USER";
    public const string MatchReport = "MATCH_REPORT";
    public const string MediaItem = "MEDIA_ITEM";
    public const string ImportJob = "IMPORT_JOB";
    public static bool IsKnown(string value) => value is StaffUser or MatchReport or MediaItem or ImportJob;
}
public static class AuditActions
{
    public const string StaffInvitationCreated = "STAFF_INVITATION_CREATED", StaffInvitationReissued = "STAFF_INVITATION_REISSUED", StaffInvitationAccepted = "STAFF_INVITATION_ACCEPTED", StaffProfileUpdated = "STAFF_PROFILE_UPDATED", StaffAccessReplaced = "STAFF_ACCESS_REPLACED", StaffUserDisabled = "STAFF_USER_DISABLED", StaffUserReactivated = "STAFF_USER_REACTIVATED", MatchReportCreated = "MATCH_REPORT_CREATED", MatchReportSubmitted = "MATCH_REPORT_SUBMITTED", MatchReportVerified = "MATCH_REPORT_VERIFIED", MatchReportCorrectionRequested = "MATCH_REPORT_CORRECTION_REQUESTED", MatchReportArchived = "MATCH_REPORT_ARCHIVED", MatchReportStatisticsUpdated = "MATCH_REPORT_STATISTICS_UPDATED", MediaItemCreated = "MEDIA_ITEM_CREATED", MediaItemUpdated = "MEDIA_ITEM_UPDATED", MediaItemLinked = "MEDIA_ITEM_LINKED", MediaItemUnlinked = "MEDIA_ITEM_UNLINKED", MediaItemArchived = "MEDIA_ITEM_ARCHIVED", MediaItemRestored = "MEDIA_ITEM_RESTORED", ImportJobCreated = "IMPORT_JOB_CREATED", ImportJobPreviewed = "IMPORT_JOB_PREVIEW_GENERATED", ImportJobValidated = "IMPORT_JOB_VALIDATED", ImportJobConfirmed = "IMPORT_JOB_CONFIRMED", ImportJobFailed = "IMPORT_JOB_PROCESSING_FAILED", ImportJobCancelled = "IMPORT_JOB_CANCELLED";
    private static readonly HashSet<string> Values = [StaffInvitationCreated, StaffInvitationReissued, StaffInvitationAccepted, StaffProfileUpdated, StaffAccessReplaced, StaffUserDisabled, StaffUserReactivated, MatchReportCreated, MatchReportSubmitted, MatchReportVerified, MatchReportCorrectionRequested, MatchReportArchived, MatchReportStatisticsUpdated, MediaItemCreated, MediaItemUpdated, MediaItemLinked, MediaItemUnlinked, MediaItemArchived, MediaItemRestored, ImportJobCreated, ImportJobPreviewed, ImportJobValidated, ImportJobConfirmed, ImportJobFailed, ImportJobCancelled];
    public static bool IsKnown(string value) => Values.Contains(value);
}
