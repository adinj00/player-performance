using PlayerPerformance.Domain.Matches;

namespace PlayerPerformance.UnitTests.Matches;

public sealed class MatchTests
{
    private static readonly DateTime Now = new(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_ShouldDefaultToScheduledWithoutScores()
    {
        var match = Create();
        Assert.Equal(MatchStatus.SCHEDULED, match.Status);
        Assert.Null(match.TeamScore);
    }

    [Fact]
    public void Update_ShouldRequireScoresOnlyForPlayedMatch()
    {
        var match = Create();
        Assert.Throws<InvalidOperationException>(() => Update(match, MatchStatus.PLAYED, null, null));
        Assert.Throws<InvalidOperationException>(() => Update(match, MatchStatus.SCHEDULED, 1, 0));
    }

    [Fact]
    public void Update_ShouldAllowPlayedScoreCorrectionButNotReopening()
    {
        var match = Create();
        Update(match, MatchStatus.PLAYED, 1, 0);
        Update(match, MatchStatus.PLAYED, 2, 0);
        Assert.Equal(2, match.TeamScore);
        Assert.Throws<InvalidOperationException>(() => Update(match, MatchStatus.SCHEDULED, null, null));
    }

    [Fact]
    public void ArchiveAndRestore_ShouldPreserveMatchStatus()
    {
        var match = Create();
        Update(match, MatchStatus.POSTPONED, null, null);
        match.Archive(Now.AddMinutes(1));
        match.Restore(Now.AddMinutes(2));
        Assert.False(match.IsArchived);
        Assert.Equal(MatchStatus.POSTPONED, match.Status);
    }

    private static Match Create() => Match.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, Now, null, MatchLocationType.HOME, Now);
    private static void Update(Match match, MatchStatus status, int? teamScore, int? opponentScore) => match.UpdateMetadata(match.SeasonId, match.CompetitionId, match.OpponentId, match.VenueId, match.KickoffAtUtc, match.Round, match.LocationType, status, teamScore, opponentScore, Now.AddMinutes(1));
}
