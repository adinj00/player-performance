using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;

namespace PlayerPerformance.Domain.Matches;

public sealed class Match : Entity
{
    private Match() : base(Guid.Empty)
    {
    }
    private Match(Guid id, Guid seasonId, Guid competitionId, Guid teamId, Guid opponentId, Guid? venueId, DateTime kickoffAtUtc, string? round, MatchLocationType locationType, DateTime utcNow) : base(id)
    {
        SeasonId = seasonId;
        CompetitionId = competitionId;
        TeamId = teamId;
        OpponentId = opponentId;
        VenueId = venueId;
        KickoffAtUtc = kickoffAtUtc;
        Round = round;
        LocationType = locationType;
        Status = MatchStatus.SCHEDULED;
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }
    public Guid SeasonId { get; private set; }
    public Guid CompetitionId { get; private set; }
    public Guid TeamId { get; }
    public Guid OpponentId { get; private set; }
    public Guid? VenueId { get; private set; }
    public DateTime KickoffAtUtc { get; private set; }
    public string? Round { get; private set; }
    public MatchLocationType LocationType { get; private set; }
    public MatchStatus Status { get; private set; }
    public int? TeamScore { get; private set; }
    public int? OpponentScore { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public static Match Create(Guid id, Guid seasonId, Guid competitionId, Guid teamId, Guid opponentId, Guid? venueId, DateTime kickoffAtUtc, string? round, MatchLocationType locationType, DateTime utcNow)
    {
        EnsureIds(seasonId, competitionId, teamId, opponentId);
        EnsureKickoff(kickoffAtUtc);
        EnsureLocation(locationType);
        return new(id, seasonId, competitionId, teamId, opponentId, venueId, kickoffAtUtc, NormalizeRound(round), locationType, utcNow);
    }
    public void UpdateMetadata(Guid seasonId, Guid competitionId, Guid opponentId, Guid? venueId, DateTime kickoffAtUtc, string? round, MatchLocationType locationType, MatchStatus status, int? teamScore, int? opponentScore, DateTime utcNow)
    {
        EnsureIds(seasonId, competitionId, TeamId, opponentId);
        EnsureKickoff(kickoffAtUtc);
        EnsureLocation(locationType);
        EnsureScores(status, teamScore, opponentScore);
        ChangeStatus(status);
        SeasonId = seasonId;
        CompetitionId = competitionId;
        OpponentId = opponentId;
        VenueId = venueId;
        KickoffAtUtc = kickoffAtUtc;
        Round = NormalizeRound(round);
        LocationType = locationType;
        TeamScore = teamScore;
        OpponentScore = opponentScore;
        UpdatedAtUtc = utcNow;
    }
    public void Archive(DateTime utcNow)
    {
        if (!IsArchived)
        {
            IsArchived = true;
            UpdatedAtUtc = utcNow;
        }
    }

    public void Restore(DateTime utcNow)
    {
        if (IsArchived)
        {
            IsArchived = false;
            UpdatedAtUtc = utcNow;
        }
    }
    private void ChangeStatus(MatchStatus status)
    {
        if (!Enum.IsDefined(status))
            throw new ArgumentOutOfRangeException(nameof(status));
        if (status == Status)
            return;
        var allowed = (Status, status) is (MatchStatus.SCHEDULED, MatchStatus.PLAYED or MatchStatus.POSTPONED or MatchStatus.CANCELLED) or (MatchStatus.POSTPONED, MatchStatus.SCHEDULED or MatchStatus.CANCELLED);
        if (!allowed)
            throw new InvalidOperationException("The requested match status transition is not allowed.");
        Status = status;
    }
    private static void EnsureIds(Guid seasonId, Guid competitionId, Guid teamId, Guid opponentId)
    {
        Guard.AgainstDefault(seasonId, nameof(seasonId));
        Guard.AgainstDefault(competitionId, nameof(competitionId));
        Guard.AgainstDefault(teamId, nameof(teamId));
        Guard.AgainstDefault(opponentId, nameof(opponentId));
    }

    private static void EnsureKickoff(DateTime kickoffAtUtc)
    {
        if (kickoffAtUtc == default || kickoffAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentOutOfRangeException(nameof(kickoffAtUtc));
        }
    }

    private static void EnsureLocation(MatchLocationType locationType)
    {
        if (!Enum.IsDefined(locationType))
        {
            throw new ArgumentOutOfRangeException(nameof(locationType));
        }
    }

    private static void EnsureScores(MatchStatus status, int? teamScore, int? opponentScore)
    {
        if (teamScore < 0 || opponentScore < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(teamScore));
        }

        if (teamScore.HasValue != opponentScore.HasValue || (status == MatchStatus.PLAYED) != teamScore.HasValue)
        {
            throw new InvalidOperationException("Final scores must be present together only for played matches.");
        }
    }
    private static string? NormalizeRound(string? round) => string.IsNullOrWhiteSpace(round) ? null : round.Trim();
}
