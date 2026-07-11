using PlayerPerformance.Domain.Players;

namespace PlayerPerformance.UnitTests.Players;

public sealed class PlayerTeamAssignmentTests
{
    [Fact]
    public void Create_ShouldSupportHistoricalAndOpenEndedDateRanges()
    {
        var now = new DateTime(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);
        var historical = PlayerTeamAssignment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2025, 1, 1), new DateOnly(2025, 6, 30), now);
        var openEnded = PlayerTeamAssignment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 7, 1), null, now);

        Assert.Equal(new DateOnly(2025, 6, 30), historical.EndDate);
        Assert.Null(openEnded.EndDate);
    }

    [Fact]
    public void Create_ShouldRejectEndDateBeforeStartDate() => Assert.Throws<ArgumentOutOfRangeException>(() => PlayerTeamAssignment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 7, 2), new DateOnly(2026, 7, 1), DateTime.UtcNow));

    [Fact]
    public void End_ShouldEndOpenAssignmentAndRejectRepeatedEnd()
    {
        var assignment = PlayerTeamAssignment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 1, 1), null, DateTime.UtcNow);
        assignment.End(new DateOnly(2026, 7, 11), DateTime.UtcNow);

        Assert.Equal(new DateOnly(2026, 7, 11), assignment.EndDate);
        Assert.Throws<InvalidOperationException>(() => assignment.End(new DateOnly(2026, 7, 12), DateTime.UtcNow));
    }
}
