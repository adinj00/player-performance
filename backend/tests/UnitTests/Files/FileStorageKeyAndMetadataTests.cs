using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Application.Files;
using PlayerPerformance.Domain.Files;

namespace PlayerPerformance.UnitTests.Files;

public sealed class FileStorageKeyAndMetadataTests
{
    [Fact]
    public void Create_ShouldGenerateOpaqueUniqueValidKeys()
    {
        var generator = new FileStorageKeyGenerator(new FixedClock(new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc)));

        var keys = Enumerable.Range(0, 500).Select(_ => generator.Create()).ToArray();

        Assert.All(keys, key => Assert.Matches("^objects/2026/07/[a-f0-9]{32}$", key));
        Assert.Equal(keys.Length, keys.Distinct(StringComparer.Ordinal).Count());
        Assert.All(keys, key => Assert.True(FileStorageKeyValidator.IsValid(key)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("/objects/a")]
    [InlineData("objects/../a")]
    [InlineData("objects\\a")]
    [InlineData("C:/objects/a")]
    [InlineData("https://example.test/a")]
    public void IsValid_ShouldRejectUnsafeKeys(string key)
    {
        Assert.False(FileStorageKeyValidator.IsValid(key));
    }

    [Theory]
    [InlineData("report.pdf", "report.pdf")]
    [InlineData("C:\\temp\\report.pdf", "report.pdf")]
    [InlineData("/tmp/report.pdf", "report.pdf")]
    [InlineData("  report.pdf  ", "report.pdf")]
    public void NormalizeOriginalFileName_ShouldKeepOnlyFinalSafeComponent(string input, string expected)
    {
        var result = FileMetadataValidation.NormalizeOriginalFileName(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t ")]
    [InlineData("bad\u0000name")]
    public void NormalizeOriginalFileName_ShouldRejectUnsafeNames(string input)
    {
        Assert.True(FileMetadataValidation.NormalizeOriginalFileName(input).IsFailure);
    }

    [Fact]
    public void StoredFile_ShouldRequireValidMetadataAndPreserveObjectWhenArchived()
    {
        var storedFile = StoredFile.Create(Guid.NewGuid(), "objects/2026/07/object", "report.pdf", "application/pdf", 42, Guid.NewGuid(), DateTime.UtcNow);
        var key = storedFile.StorageKey;

        storedFile.Archive(Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(storedFile.IsArchived);
        Assert.Equal(key, storedFile.StorageKey);
        Assert.NotNull(storedFile.ArchivedAtUtc);
        Assert.NotNull(storedFile.ArchivedByUserId);
        Assert.Throws<ArgumentOutOfRangeException>(() => StoredFile.Create(Guid.NewGuid(), "objects/a", "x", "text/plain", 0, Guid.NewGuid(), DateTime.UtcNow));
    }

    private sealed class FixedClock(DateTime utcNow) : ISystemClock
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
