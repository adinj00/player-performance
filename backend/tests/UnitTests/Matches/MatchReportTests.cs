using PlayerPerformance.Domain.Matches;

namespace PlayerPerformance.UnitTests.Matches;

public sealed class MatchReportTests
{
    private static readonly DateTime Now = new(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_ShouldStartAsDraft()
    {
        var report = Create();
        Assert.Equal(MatchReportStatus.DRAFT, report.Status);
    }

    [Fact]
    public void Workflow_ShouldAllowSubmitVerifyCorrectionAndResubmit()
    {
        var report = Create();
        report.Submit(Guid.NewGuid(), Now.AddMinutes(1));
        report.Verify(Guid.NewGuid(), Now.AddMinutes(2));
        report.RequestCorrection(Guid.NewGuid(), "  Dopuniti   zapisnik. ", Now.AddMinutes(3));
        report.Submit(Guid.NewGuid(), Now.AddMinutes(4));
        Assert.Equal(MatchReportStatus.READY_FOR_REVIEW, report.Status);
        Assert.Equal("Dopuniti zapisnik.", report.LastCorrectionReason);
        Assert.NotNull(report.SubmittedAtUtc);
    }

    [Fact]
    public void InvalidTransition_AndInvalidCorrectionReason_ShouldBeRejected()
    {
        var report = Create();
        Assert.Throws<InvalidOperationException>(() => report.Verify(Guid.NewGuid(), Now));
        report.Submit(Guid.NewGuid(), Now);
        Assert.Throws<ArgumentOutOfRangeException>(() => report.RequestCorrection(Guid.NewGuid(), " ", Now));
    }

    [Fact]
    public void Archive_ShouldBeTerminalAndRecordMetadata()
    {
        var report = Create();
        var verifier = Guid.NewGuid();
        var archiver = Guid.NewGuid();
        report.Submit(Guid.NewGuid(), Now);
        report.Verify(verifier, Now.AddMinutes(1));
        report.Archive(archiver, Now.AddMinutes(2));
        Assert.Equal(MatchReportStatus.ARCHIVED, report.Status);
        Assert.Equal(archiver, report.ArchivedByUserId);
        Assert.NotNull(report.ArchivedAtUtc);
        Assert.Throws<InvalidOperationException>(() => report.RequestCorrection(Guid.NewGuid(), "Reason", Now.AddMinutes(3)));
    }

    private static MatchReport Create() => MatchReport.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Now);
}
