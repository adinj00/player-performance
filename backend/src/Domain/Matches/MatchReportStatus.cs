namespace PlayerPerformance.Domain.Matches;

public enum MatchReportStatus
{
    DRAFT = 1,
    READY_FOR_REVIEW = 2,
    VERIFIED = 3,
    NEEDS_CORRECTION = 4,
    ARCHIVED = 5
}
