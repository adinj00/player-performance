using Microsoft.Extensions.Options;
using PlayerPerformance.Application.Files;
using PlayerPerformance.Infrastructure.Files;

namespace PlayerPerformance.UnitTests.Files;

public sealed class LocalFileStorageTests : IDisposable
{
    private readonly string rootPath = Path.Combine(Path.GetTempPath(), "player-performance-file-storage", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task WriteAndOpenRead_ShouldStreamActualBytesWithoutClosingInput()
    {
        var storage = CreateStorage(10);
        await using var input = new MemoryStream([1, 2, 3]);

        var write = await storage.WriteAsync(new FileStorageWriteRequest(input, "objects/2026/07/one", 3), CancellationToken.None);
        var read = await storage.OpenReadAsync("objects/2026/07/one", CancellationToken.None);

        Assert.True(write.IsSuccess);
        Assert.Equal(3, write.Value.SizeBytes);
        Assert.True(input.CanRead);
        Assert.True(read.IsSuccess);
        await using var output = read.Value;
        Assert.Equal([1, 2, 3], await ReadAllAsync(output));
    }

    [Fact]
    public async Task Write_ShouldRejectCollisionTraversalAndBoundaryViolations()
    {
        var storage = CreateStorage(3);
        await using var exactLimit = new MemoryStream([1, 2, 3]);
        await using var oversized = new MemoryStream([1, 2, 3, 4]);
        await using var empty = new MemoryStream();

        var accepted = await storage.WriteAsync(new FileStorageWriteRequest(exactLimit, "objects/2026/07/boundary", null), CancellationToken.None);
        var collision = await storage.WriteAsync(new FileStorageWriteRequest(new MemoryStream([1]), "objects/2026/07/boundary", 1), CancellationToken.None);
        var tooLarge = await storage.WriteAsync(new FileStorageWriteRequest(oversized, "objects/2026/07/large", null), CancellationToken.None);
        var zero = await storage.WriteAsync(new FileStorageWriteRequest(empty, "objects/2026/07/empty", 0), CancellationToken.None);
        var traversal = await storage.WriteAsync(new FileStorageWriteRequest(new MemoryStream([1]), "../outside", 1), CancellationToken.None);

        Assert.True(accepted.IsSuccess);
        Assert.Equal("file_storage.objectalreadyexists", collision.Error.Code);
        Assert.Equal("file_storage.objecttoolarge", tooLarge.Error.Code);
        Assert.Equal("file_storage.emptycontent", zero.Error.Code);
        Assert.Equal("file_storage.invalidstoragekey", traversal.Error.Code);
        Assert.False(File.Exists(Path.Combine(rootPath, "outside")));
    }

    [Fact]
    public async Task DeleteIfExists_ShouldBeIdempotent()
    {
        var storage = CreateStorage(10);
        await using var input = new MemoryStream([1]);
        await storage.WriteAsync(new FileStorageWriteRequest(input, "objects/2026/07/delete", 1), CancellationToken.None);

        var first = await storage.DeleteIfExistsAsync("objects/2026/07/delete", CancellationToken.None);
        var second = await storage.DeleteIfExistsAsync("objects/2026/07/delete", CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
    }

    [Fact]
    public async Task Write_ShouldRejectDeclaredOversizeAndCancellationWithoutCreatingFinalObject()
    {
        var storage = CreateStorage(3);
        await using var oversized = new MemoryStream([1, 2, 3, 4]);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();

        var early = await storage.WriteAsync(new FileStorageWriteRequest(oversized, "objects/2026/07/early", 4), CancellationToken.None);
        var cancellation = await storage.WriteAsync(new FileStorageWriteRequest(new MemoryStream([1]), "objects/2026/07/cancelled", 1), cancelled.Token);

        Assert.Equal("file_storage.objecttoolarge", early.Error.Code);
        Assert.Equal("file_storage.operationcancelled", cancellation.Error.Code);
        Assert.False(File.Exists(Path.Combine(rootPath, "objects", "2026", "07", "early")));
        Assert.False(File.Exists(Path.Combine(rootPath, "objects", "2026", "07", "cancelled")));
        Assert.Empty(Directory.Exists(Path.Combine(rootPath, ".temporary")) ? [] : Directory.EnumerateFiles(Path.Combine(rootPath, ".temporary"), "*", SearchOption.AllDirectories));
    }

    [Fact]
    public async Task OpenRead_ShouldReturnSafeMissingObjectFailure()
    {
        var result = await CreateStorage(10).OpenReadAsync("objects/2026/07/missing", CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("file_storage.objectnotfound", result.Error.Code);
    }

    public void Dispose()
    {
        if (Directory.Exists(rootPath))
        {
            Directory.Delete(rootPath, true);
        }
    }

    private LocalFileStorage CreateStorage(long maximumSize) => new(Options.Create(new FileStorageOptions
    {
        MaxObjectSizeBytes = maximumSize,
        ResolvedLocalRootPath = rootPath
    }));

    private static async Task<byte[]> ReadAllAsync(Stream stream)
    {
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer);
        return buffer.ToArray();
    }
}
