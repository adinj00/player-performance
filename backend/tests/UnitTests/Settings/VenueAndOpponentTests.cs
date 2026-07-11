using PlayerPerformance.Domain.Settings;

namespace PlayerPerformance.UnitTests.Settings;

public sealed class VenueAndOpponentTests
{
    [Fact]
    public void Venue_UpdateAfterArchive_ShouldBeRejected()
    {
        var venue = Venue.Create(Guid.NewGuid(), "Stadium", "STADIUM", DateTime.UtcNow);
        venue.Archive(DateTime.UtcNow);

        Assert.Throws<InvalidOperationException>(() => venue.Update("New Stadium", "NEW STADIUM", DateTime.UtcNow));
    }

    [Fact]
    public void Opponent_ArchiveAndRestore_ShouldBeIdempotent()
    {
        var opponent = Opponent.Create(Guid.NewGuid(), "FK Example", "FK EXAMPLE", DateTime.UtcNow);

        opponent.Archive(DateTime.UtcNow);
        opponent.Archive(DateTime.UtcNow);
        opponent.Restore(DateTime.UtcNow);
        opponent.Restore(DateTime.UtcNow);

        Assert.False(opponent.IsArchived);
    }
}
