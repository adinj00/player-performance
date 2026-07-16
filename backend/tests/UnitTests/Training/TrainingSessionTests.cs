using PlayerPerformance.Domain.Training;

namespace PlayerPerformance.UnitTests.Training;

public sealed class TrainingSessionTests
{
    private static readonly Guid Team = Guid.NewGuid(), Actor = Guid.NewGuid();
    [Fact]
    public void Create_StartsPlanned_AndValidatesTimeOrder()
    {
        var now = DateTime.UtcNow;
        var session = TrainingSession.Create(Guid.NewGuid(), Team, new DateOnly(2026, 7, 16), now, now.AddHours(2), "Teretana", null, null, Actor, now);
        Assert.Equal(TrainingSessionStatus.PLANNED, session.Status);
        Assert.Throws<ArgumentOutOfRangeException>(() => TrainingSession.Create(Guid.NewGuid(), Team, new DateOnly(2026, 7, 16), now, now, "Teretana", null, null, Actor, now));
    }
    [Fact]
    public void CompletedSession_AllowsDescriptiveCorrection_ButNotTimeChange()
    {
        var now = DateTime.UtcNow;
        var session = TrainingSession.Create(Guid.NewGuid(), Team, new DateOnly(2026, 7, 16), null, null, "A", null, null, Actor, now);
        session.Complete(Actor, now.AddMinutes(1));
        Assert.True(session.Update(session.SessionDate, null, null, "B", "Mostar", null, now.AddMinutes(2)));
        Assert.Throws<InvalidOperationException>(() => session.Update(session.SessionDate.AddDays(1), null, null, "B", null, null, now));
        Assert.Throws<InvalidOperationException>(() => session.Cancel(Actor, now));
    }
    [Fact]
    public void Participant_RemovalPreservesHistory()
    {
        var participant = TrainingSessionParticipant.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Actor, DateTime.UtcNow);
        participant.Remove(Actor, DateTime.UtcNow.AddMinutes(1));
        Assert.False(participant.IsActive);
        Assert.NotNull(participant.RemovedAtUtc);
        Assert.Throws<InvalidOperationException>(() => participant.Remove(Actor, DateTime.UtcNow));
    }
}
