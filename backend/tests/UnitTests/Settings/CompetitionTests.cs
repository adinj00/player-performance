using PlayerPerformance.Domain.Settings;

namespace PlayerPerformance.UnitTests.Settings;

public sealed class CompetitionTests
{
    [Fact]
    public void ArchiveAndRestore_ShouldBeIdempotent()
    {
        var competition = Competition.Create(Guid.NewGuid(), "League", "LEAGUE", DateTime.UtcNow);
        competition.Archive(DateTime.UtcNow);
        competition.Archive(DateTime.UtcNow);
        Assert.True(competition.IsArchived);
        competition.Restore(DateTime.UtcNow);
        competition.Restore(DateTime.UtcNow);
        Assert.False(competition.IsArchived);
    }
}
