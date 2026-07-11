using PlayerPerformance.Application.Matches;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Domain.Teams;

namespace PlayerPerformance.UnitTests.Matches;

public sealed class MatchStatisticsTests
{
    [Theory]
    [InlineData(TeamTrackingLevel.BASIC, 4, 3)]
    [InlineData(TeamTrackingLevel.STANDARD, 9, 4)]
    [InlineData(TeamTrackingLevel.FULL, 16, 4)]
    public void TrackingProfiles_ShouldExposeTheConfirmedFieldMatrix(TeamTrackingLevel level, int playerCount, int goalkeeperCount)
    {
        var profile = new MatchStatisticsProfileService().Get(level);
        Assert.Equal(playerCount, profile.PlayerFields.Count);
        Assert.Equal(goalkeeperCount, profile.GoalkeeperFields.Count);
    }

    [Fact]
    public void AppliedTrackingLevel_ShouldRemainImmutable()
    {
        var report = MatchReport.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        report.ApplyTrackingLevel(TeamTrackingLevel.BASIC, DateTime.UtcNow);
        report.ApplyTrackingLevel(TeamTrackingLevel.FULL, DateTime.UtcNow.AddMinutes(1));
        Assert.Equal(TeamTrackingLevel.BASIC, report.AppliedTrackingLevel);
    }

    [Fact]
    public void PlayerValues_ShouldKeepNullDistinctFromZero_AndRejectInvalidRelationships()
    {
        var zero = new PlayerMatchStatsValues(0, null, null, null, 0, 0, null, null, null, null, null, null, null, null, null, null);
        PlayerMatchStats.Validate(zero);
        Assert.Equal(0, zero.Goals);
        Assert.Null(zero.Assists);
        Assert.Throws<ArgumentOutOfRangeException>(() => PlayerMatchStats.Validate(new(null, null, null, null, 1, 2, null, null, null, null, null, null, null, null, null, null)));
        Assert.Throws<ArgumentOutOfRangeException>(() => PlayerMatchStats.Validate(new(null, null, null, null, null, null, 1, 2, null, null, null, null, null, null, null, null)));
        Assert.Throws<ArgumentOutOfRangeException>(() => PlayerMatchStats.Validate(new(null, null, null, null, null, null, null, null, null, 1, 2, null, null, null, null, null)));
        Assert.Throws<ArgumentOutOfRangeException>(() => PlayerMatchStats.Validate(new(-1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)));
    }
}
