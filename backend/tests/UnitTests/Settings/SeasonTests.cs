using PlayerPerformance.Domain.Settings;

namespace PlayerPerformance.UnitTests.Settings;

public sealed class SeasonTests
{
    [Fact]
    public void Create_ShouldRejectAnInvalidDateRange() => Assert.Throws<ArgumentException>(() => Season.Create(Guid.NewGuid(), "2026/27", "2026/27", new DateOnly(2027, 6, 1), new DateOnly(2026, 7, 1), DateTime.UtcNow));

    [Fact]
    public void ArchiveAndRestore_ShouldBeIdempotent()
    {
        var season = Season.Create(Guid.NewGuid(), "2026/27", "2026/27", new DateOnly(2026, 7, 1), new DateOnly(2027, 6, 1), DateTime.UtcNow);
        season.Archive(DateTime.UtcNow);
        season.Archive(DateTime.UtcNow);
        Assert.True(season.IsArchived);
        season.Restore(DateTime.UtcNow);
        season.Restore(DateTime.UtcNow);
        Assert.False(season.IsArchived);
    }
}
