using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;

namespace PlayerPerformance.Domain.Matches;

public sealed class MatchReport : Entity
{
    public const int MaxCorrectionReasonLength = 1000;

    private MatchReport() : base(Guid.Empty) { }

    private MatchReport(Guid id, Guid matchId, Guid createdByUserId, DateTime utcNow) : base(id)
    {
        MatchId = matchId;
        CreatedByUserId = createdByUserId;
        Status = MatchReportStatus.DRAFT;
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public Guid MatchId { get; private set; }
    public MatchReportStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public Guid? SubmittedByUserId { get; private set; }
    public DateTime? SubmittedAtUtc { get; private set; }
    public Guid? VerifiedByUserId { get; private set; }
    public DateTime? VerifiedAtUtc { get; private set; }
    public Guid? LastCorrectionRequestedByUserId { get; private set; }
    public DateTime? LastCorrectionRequestedAtUtc { get; private set; }
    public string? LastCorrectionReason { get; private set; }
    public Guid? ArchivedByUserId { get; private set; }
    public DateTime? ArchivedAtUtc { get; private set; }

    public static MatchReport Create(Guid id, Guid matchId, Guid createdByUserId, DateTime utcNow)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(matchId, nameof(matchId));
        Guard.AgainstDefault(createdByUserId, nameof(createdByUserId));
        return new(id, matchId, createdByUserId, utcNow);
    }

    public void Submit(Guid actorUserId, DateTime utcNow)
    {
        TransitionTo(MatchReportStatus.READY_FOR_REVIEW, MatchReportStatus.DRAFT, MatchReportStatus.NEEDS_CORRECTION);
        SubmittedByUserId = RequiredActor(actorUserId);
        SubmittedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }
    public void Verify(Guid actorUserId, DateTime utcNow)
    {
        TransitionTo(MatchReportStatus.VERIFIED, MatchReportStatus.READY_FOR_REVIEW);
        VerifiedByUserId = RequiredActor(actorUserId);
        VerifiedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }
    public void RequestCorrection(Guid actorUserId, string reason, DateTime utcNow)
    {
        TransitionTo(MatchReportStatus.NEEDS_CORRECTION, MatchReportStatus.READY_FOR_REVIEW, MatchReportStatus.VERIFIED);
        LastCorrectionRequestedByUserId = RequiredActor(actorUserId);
        LastCorrectionRequestedAtUtc = utcNow;
        LastCorrectionReason = NormalizeReason(reason);
        UpdatedAtUtc = utcNow;
    }
    public void Archive(Guid actorUserId, DateTime utcNow)
    {
        TransitionTo(MatchReportStatus.ARCHIVED, MatchReportStatus.VERIFIED);
        ArchivedByUserId = RequiredActor(actorUserId);
        ArchivedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    private void TransitionTo(MatchReportStatus target, params MatchReportStatus[] allowed)
    {
        if (!allowed.Contains(Status))
            throw new InvalidOperationException("The requested report workflow transition is not allowed.");
        Status = target;
    }
    private static Guid RequiredActor(Guid actorUserId)
    {
        Guard.AgainstDefault(actorUserId, nameof(actorUserId));
        return actorUserId;
    }
    private static string NormalizeReason(string reason)
    {
        var normalized = string.IsNullOrWhiteSpace(reason) ? string.Empty : string.Join(' ', reason.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (normalized.Length == 0 || normalized.Length > MaxCorrectionReasonLength)
            throw new ArgumentOutOfRangeException(nameof(reason));
        return normalized;
    }
}
