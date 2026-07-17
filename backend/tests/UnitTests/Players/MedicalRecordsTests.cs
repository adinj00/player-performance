using PlayerPerformance.Domain.Medical;

namespace PlayerPerformance.UnitTests.Players;

public sealed class MedicalRecordsTests
{
    [Fact]
    public void AvailabilityRevision_ShouldNormalizeNote_AndEnforceExpectedReturnRules()
    {
        var revision = PlayerAvailabilityRevision.Create(Guid.NewGuid(), Guid.NewGuid(), 1, AvailabilityStatus.LIMITED, new DateOnly(2026, 7, 17), new DateOnly(2026, 7, 18), "  Individual programme  ", Guid.NewGuid(), DateTime.UtcNow);

        Assert.Equal("Individual programme", revision.CoachVisibleNote);
        Assert.Throws<ArgumentOutOfRangeException>(() => PlayerAvailabilityRevision.Create(Guid.NewGuid(), Guid.NewGuid(), 1, AvailabilityStatus.AVAILABLE, new DateOnly(2026, 7, 17), new DateOnly(2026, 7, 18), null, Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void InjuryRevision_ShouldRequireAtLeastOneRestrictedDetail_AndTrimValues()
    {
        var revision = InjuryRecordRevision.Create(Guid.NewGuid(), Guid.NewGuid(), 1, "  knee ", null, null, Guid.NewGuid(), DateTime.UtcNow);

        Assert.Equal("knee", revision.BodyArea);
        Assert.Throws<ArgumentOutOfRangeException>(() => InjuryRecordRevision.Create(Guid.NewGuid(), Guid.NewGuid(), 1, null, null, " ", Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void InjuryRecord_ShouldResolveOnce_AndRetainImmutableOccurrence()
    {
        var occurred = new DateOnly(2026, 7, 10);
        var record = InjuryRecord.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), occurred, Guid.NewGuid(), DateTime.UtcNow);

        record.Resolve(new DateOnly(2026, 7, 12), Guid.NewGuid(), DateTime.UtcNow);

        Assert.Equal(InjuryStatus.RESOLVED, record.Status);
        Assert.Equal(occurred, record.OccurredOn);
        Assert.Throws<InvalidOperationException>(() => record.Resolve(new DateOnly(2026, 7, 13), Guid.NewGuid(), DateTime.UtcNow));
    }
}
