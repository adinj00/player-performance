using PlayerPerformance.Domain.Teams;

namespace PlayerPerformance.UnitTests.Teams;

public sealed class TeamTests
{
    [Fact]
    public void Create_ShouldTrimNameAndSetInitialState()
    {
        var now = DateTime.UtcNow;
        var team = Team.Create(Guid.NewGuid(), "  U19  ", "U19", TeamTrackingLevel.STANDARD, 0, now);
        Assert.Equal("U19", team.Name);
        Assert.Equal(TeamStatus.ACTIVE, team.Status);
        Assert.Equal(0, team.DisplayOrder);
        Assert.Equal(now, team.CreatedAtUtc);
        Assert.Equal(now, team.UpdatedAtUtc);
    }

    [Fact]
    public void Lifecycle_ShouldFollowRequiredTransitionsAndBeIdempotent()
    {
        var team = Team.Create(Guid.NewGuid(), "U17", "U17", TeamTrackingLevel.STANDARD, 1, DateTime.UtcNow);
        team.Deactivate(DateTime.UtcNow);
        team.Deactivate(DateTime.UtcNow);
        Assert.Equal(TeamStatus.INACTIVE, team.Status);
        team.Archive(DateTime.UtcNow);
        team.Archive(DateTime.UtcNow);
        Assert.Equal(TeamStatus.ARCHIVED, team.Status);
        team.Restore(DateTime.UtcNow);
        team.Restore(DateTime.UtcNow);
        Assert.Equal(TeamStatus.INACTIVE, team.Status);
        team.Activate(DateTime.UtcNow);
        Assert.Equal(TeamStatus.ACTIVE, team.Status);
    }

    [Fact]
    public void ArchivedTeam_ShouldRejectNormalUpdateAndOrdering()
    {
        var team = Team.Create(Guid.NewGuid(), "U15", "U15", TeamTrackingLevel.BASIC, 2, DateTime.UtcNow);
        team.Archive(DateTime.UtcNow);
        Assert.Throws<InvalidOperationException>(() => team.Update("U15 Updated", "U15 UPDATED", TeamTrackingLevel.FULL, DateTime.UtcNow));
        Assert.Throws<InvalidOperationException>(() => team.AssignDisplayOrder(0, DateTime.UtcNow));
    }

    [Fact]
    public void Create_ShouldRejectUndefinedTrackingLevel()
        => Assert.Throws<ArgumentOutOfRangeException>(() => Team.Create(Guid.NewGuid(), "U13", "U13", (TeamTrackingLevel) 999, 0, DateTime.UtcNow));
}
