using PlayerPerformance.Domain.Imports;

namespace PlayerPerformance.UnitTests;

public sealed class ImportsTests
{
    private static readonly DateTime Now = new(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_ShouldRequireMatchingTargetAndOtherSourceLabelRules()
    {
        Assert.Throws<ArgumentException>(() => Create(ImportType.MATCH_GPS));
        Assert.Throws<ArgumentException>(() => Create(ImportType.PLAYER_ROSTER, Guid.NewGuid()));
        Assert.Throws<ArgumentException>(() => Create(source: ImportSourceSystem.OTHER));
        Assert.Throws<ArgumentException>(() => Create(sourceLabel: "unexpected"));
        var job = Create(source: ImportSourceSystem.OTHER, sourceLabel: "Zone export");
        Assert.Equal(ImportJobStatus.UPLOADED, job.Status);
        Assert.Equal("Zone export", job.SourceLabel);
        Assert.Equal(1, job.ConfigurationRevision);
    }

    [Fact]
    public void PreviewLease_ShouldPreventActiveReplacement_AndRecordBoundedPreviewCount()
    {
        var job = Create();
        var lease = job.AcquireLease(ImportProcessingOperation.PREVIEW, "fake", "1", Guid.NewGuid(), Now, TimeSpan.FromMinutes(30));
        Assert.Equal(ImportJobStatus.PARSING, job.Status);
        Assert.Throws<InvalidOperationException>(() => job.AcquireLease(ImportProcessingOperation.VALIDATION, "fake", "1", Guid.NewGuid(), Now.AddMinutes(1), TimeSpan.FromMinutes(30)));
        job.CompletePreview(lease, 2, Now.AddMinutes(2));
        Assert.Equal(ImportJobStatus.UPLOADED, job.Status);
        Assert.Equal(2, job.PreviewRowCount);
        Assert.Null(job.ProcessingLeaseId);
    }

    [Fact]
    public void CompletePreview_ShouldPersistSafeMetadataAndTotalCount()
    {
        var job = Create();
        var lease = job.AcquireLease(ImportProcessingOperation.PREVIEW, "generic-csv-preview", "1.0.0", Guid.NewGuid(), Now, TimeSpan.FromMinutes(30));

        job.CompletePreview(lease, 2, 5, "{\"readerKey\":\"generic-csv-preview\"}", Now.AddMinutes(1));

        Assert.Equal(ImportJobStatus.UPLOADED, job.Status);
        Assert.Equal(2, job.PreviewRowCount);
        Assert.Equal(5, job.TotalRowCount);
        Assert.Equal("{\"readerKey\":\"generic-csv-preview\"}", job.PreviewMetadataJson);
    }

    [Fact]
    public void ValidationAndConfirmation_ShouldRequireReadyCurrentValidation()
    {
        var job = Create();
        Assert.Throws<InvalidOperationException>(() => job.AcquireLease(ImportProcessingOperation.CONFIRMATION, "fake", "1", Guid.NewGuid(), Now, TimeSpan.FromMinutes(30)));
        var validationLease = job.AcquireLease(ImportProcessingOperation.VALIDATION, "fake", "1", Guid.NewGuid(), Now, TimeSpan.FromMinutes(30));
        job.CompleteValidation(validationLease, "fake", "1", false, 3, 3, 0, 0, Now.AddMinutes(1));
        var confirmationLease = job.AcquireLease(ImportProcessingOperation.CONFIRMATION, "fake", "1", Guid.NewGuid(), Now.AddMinutes(2), TimeSpan.FromMinutes(30));
        job.CompleteConfirmation(confirmationLease, Guid.NewGuid(), "{}", Now.AddMinutes(3));
        Assert.Equal(ImportJobStatus.IMPORTED, job.Status);
        Assert.NotNull(job.ConfirmedAtUtc);
        Assert.Throws<InvalidOperationException>(() => job.Cancel(Guid.NewGuid(), Now.AddMinutes(4), TimeSpan.FromMinutes(30)));
    }

    [Fact]
    public void StaleLease_ShouldBeReplaceable_AndOldLeaseCannotFinalize()
    {
        var job = Create();
        var oldLease = job.AcquireLease(ImportProcessingOperation.PREVIEW, "fake", "1", Guid.NewGuid(), Now, TimeSpan.FromMinutes(30));
        var replacement = job.AcquireLease(ImportProcessingOperation.VALIDATION, "fake", "1", Guid.NewGuid(), Now.AddMinutes(31), TimeSpan.FromMinutes(30));
        Assert.NotEqual(oldLease, replacement);
        Assert.Throws<InvalidOperationException>(() => job.CompletePreview(oldLease, 1, Now.AddMinutes(32)));
        job.CompleteValidation(replacement, "fake", "1", true, 3, 2, 1, 0, Now.AddMinutes(32));
        Assert.Equal(ImportJobStatus.VALIDATION_FAILED, job.Status);
    }

    [Fact]
    public void Cancel_ShouldAllowStaleProcessingButRejectActiveProcessing()
    {
        var job = Create();
        job.AcquireLease(ImportProcessingOperation.PREVIEW, "fake", "1", Guid.NewGuid(), Now, TimeSpan.FromMinutes(30));
        Assert.Throws<InvalidOperationException>(() => job.Cancel(Guid.NewGuid(), Now.AddMinutes(1), TimeSpan.FromMinutes(30)));
        job.Cancel(Guid.NewGuid(), Now.AddMinutes(31), TimeSpan.FromMinutes(30));
        Assert.Equal(ImportJobStatus.CANCELLED, job.Status);
        Assert.NotNull(job.CancelledAtUtc);
        Assert.Null(job.ProcessingLeaseId);
    }

    private static ImportJob Create(ImportType type = ImportType.PLAYER_ROSTER, Guid? matchId = null, ImportSourceSystem source = ImportSourceSystem.GENERIC, string? sourceLabel = null) => ImportJob.Create(Guid.NewGuid(), Guid.NewGuid(), matchId, Guid.NewGuid(), type, source, sourceLabel, ImportFileFormat.CSV, " description ", Guid.NewGuid(), Now);
}
