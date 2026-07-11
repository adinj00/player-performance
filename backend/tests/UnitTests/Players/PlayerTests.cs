using PlayerPerformance.Domain.Players;

namespace PlayerPerformance.UnitTests.Players;

public sealed class PlayerTests
{
    [Fact]
    public void Create_ShouldTrimNamesNormalizePreferredNameAndStartActive()
    {
        var now = new DateTime(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);
        var player = Player.Create(Guid.NewGuid(), "  Amar ", "  Osmić  ", "   ", new DateOnly(2005, 2, 3), now);

        Assert.Equal("Amar", player.FirstName);
        Assert.Equal("Osmić", player.LastName);
        Assert.Null(player.PreferredName);
        Assert.Equal(PlayerRecordStatus.ACTIVE, player.Status);
        Assert.Equal(now, player.CreatedAtUtc);
    }

    [Fact]
    public void Create_ShouldRejectRequiredNamesAndFutureDateOfBirth()
    {
        var now = new DateTime(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);
        Assert.Throws<ArgumentException>(() => Player.Create(Guid.NewGuid(), " ", "Kovač", null, null, now));
        Assert.Throws<ArgumentException>(() => Player.Create(Guid.NewGuid(), "Amar", " ", null, null, now));
        Assert.Throws<ArgumentOutOfRangeException>(() => Player.Create(Guid.NewGuid(), "Amar", "Kovač", null, new DateOnly(2026, 7, 12), now));
    }

    [Fact]
    public void Lifecycle_ShouldAllowRequiredTransitionsAndRestoreInactive()
    {
        var player = Player.Create(Guid.NewGuid(), "Amar", "Kovač", "Aki", null, DateTime.UtcNow);
        player.Deactivate(DateTime.UtcNow);
        player.Archive(DateTime.UtcNow);
        player.Restore(DateTime.UtcNow);
        Assert.Equal(PlayerRecordStatus.INACTIVE, player.Status);
        player.Activate(DateTime.UtcNow);
        Assert.Equal(PlayerRecordStatus.ACTIVE, player.Status);
        player.Archive(DateTime.UtcNow);
        Assert.Equal(PlayerRecordStatus.ARCHIVED, player.Status);
    }

    [Fact]
    public void ArchivedPlayer_ShouldRejectProfileUpdateActivationAndDeactivation()
    {
        var player = Player.Create(Guid.NewGuid(), "Amar", "Kovač", null, null, DateTime.UtcNow);
        player.Archive(DateTime.UtcNow);
        Assert.Throws<InvalidOperationException>(() => player.UpdateProfile("Novi", "Igrač", null, null, DateTime.UtcNow));
        Assert.Throws<InvalidOperationException>(() => player.Activate(DateTime.UtcNow));
        Assert.Throws<InvalidOperationException>(() => player.Deactivate(DateTime.UtcNow));
    }
}
