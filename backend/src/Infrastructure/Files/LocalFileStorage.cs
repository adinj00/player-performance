using Microsoft.Extensions.Options;
using PlayerPerformance.Application.Files;
using PlayerPerformance.Domain.Common.Results;

namespace PlayerPerformance.Infrastructure.Files;

public sealed class LocalFileStorage : IFileStorage
{
    private const int BufferSize = 81920;
    private readonly string rootPath;
    private readonly long maximumObjectSizeBytes;

    public LocalFileStorage(IOptions<FileStorageOptions> options)
    {
        rootPath = options.Value.ResolvedLocalRootPath
            ?? throw new InvalidOperationException("The local file storage root was not initialized.");
        maximumObjectSizeBytes = options.Value.MaxObjectSizeBytes;
    }

    public async Task<Result<FileStorageWriteResult>> WriteAsync(FileStorageWriteRequest request, CancellationToken cancellationToken)
    {
        if (!FileStorageKeyValidator.IsValid(request.StorageKey))
        {
            return Result<FileStorageWriteResult>.Failure(FileStorageErrors.Create(FileStorageErrorCode.InvalidStorageKey));
        }

        if (request.Content is null || !request.Content.CanRead || request.DeclaredLength == 0)
        {
            return Result<FileStorageWriteResult>.Failure(FileStorageErrors.Create(FileStorageErrorCode.EmptyContent));
        }

        if (request.DeclaredLength is > 0 && request.DeclaredLength > maximumObjectSizeBytes)
        {
            return Result<FileStorageWriteResult>.Failure(FileStorageErrors.Create(FileStorageErrorCode.ObjectTooLarge));
        }

        string finalPath;
        try
        {
            finalPath = ResolveContainedPath(request.StorageKey);
        }
        catch (ArgumentException)
        {
            return Result<FileStorageWriteResult>.Failure(FileStorageErrors.Create(FileStorageErrorCode.InvalidStorageKey));
        }

        if (File.Exists(finalPath))
        {
            return Result<FileStorageWriteResult>.Failure(FileStorageErrors.Create(FileStorageErrorCode.ObjectAlreadyExists));
        }

        var temporaryPath = Path.Combine(rootPath, ".temporary", Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(finalPath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(temporaryPath)!);

            var sizeBytes = await CopyToTemporaryFileAsync(request.Content, temporaryPath, cancellationToken);
            if (sizeBytes == 0)
            {
                return Result<FileStorageWriteResult>.Failure(FileStorageErrors.Create(FileStorageErrorCode.EmptyContent));
            }

            File.Move(temporaryPath, finalPath, false);
            return Result<FileStorageWriteResult>.Success(new FileStorageWriteResult(request.StorageKey, sizeBytes));
        }
        catch (OperationCanceledException)
        {
            return Result<FileStorageWriteResult>.Failure(FileStorageErrors.Create(FileStorageErrorCode.OperationCancelled));
        }
        catch (FileStorageSizeLimitExceededException)
        {
            return Result<FileStorageWriteResult>.Failure(FileStorageErrors.Create(FileStorageErrorCode.ObjectTooLarge));
        }
        catch (IOException) when (File.Exists(finalPath))
        {
            return Result<FileStorageWriteResult>.Failure(FileStorageErrors.Create(FileStorageErrorCode.ObjectAlreadyExists));
        }
        catch (IOException)
        {
            return Result<FileStorageWriteResult>.Failure(FileStorageErrors.Create(FileStorageErrorCode.StorageUnavailable));
        }
        catch (UnauthorizedAccessException)
        {
            return Result<FileStorageWriteResult>.Failure(FileStorageErrors.Create(FileStorageErrorCode.StorageUnavailable));
        }
        finally
        {
            DeleteTemporaryFile(temporaryPath);
        }
    }

    public Task<Result<Stream>> OpenReadAsync(string storageKey, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(Result<Stream>.Failure(FileStorageErrors.Create(FileStorageErrorCode.OperationCancelled)));
        }
        if (!FileStorageKeyValidator.IsValid(storageKey))
        {
            return Task.FromResult(Result<Stream>.Failure(FileStorageErrors.Create(FileStorageErrorCode.InvalidStorageKey)));
        }

        try
        {
            var filePath = ResolveContainedPath(storageKey);
            if (!File.Exists(filePath))
            {
                return Task.FromResult(Result<Stream>.Failure(FileStorageErrors.Create(FileStorageErrorCode.ObjectNotFound)));
            }

            Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
            return Task.FromResult(Result<Stream>.Success(stream));
        }
        catch (ArgumentException)
        {
            return Task.FromResult(Result<Stream>.Failure(FileStorageErrors.Create(FileStorageErrorCode.InvalidStorageKey)));
        }
        catch (IOException)
        {
            return Task.FromResult(Result<Stream>.Failure(FileStorageErrors.Create(FileStorageErrorCode.StorageUnavailable)));
        }
        catch (UnauthorizedAccessException)
        {
            return Task.FromResult(Result<Stream>.Failure(FileStorageErrors.Create(FileStorageErrorCode.StorageUnavailable)));
        }
    }

    public Task<Result> DeleteIfExistsAsync(string storageKey, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(Result.Failure(FileStorageErrors.Create(FileStorageErrorCode.OperationCancelled)));
        }
        if (!FileStorageKeyValidator.IsValid(storageKey))
        {
            return Task.FromResult(Result.Failure(FileStorageErrors.Create(FileStorageErrorCode.InvalidStorageKey)));
        }

        try
        {
            var filePath = ResolveContainedPath(storageKey);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.FromResult(Result.Success());
        }
        catch (ArgumentException)
        {
            return Task.FromResult(Result.Failure(FileStorageErrors.Create(FileStorageErrorCode.InvalidStorageKey)));
        }
        catch (IOException)
        {
            return Task.FromResult(Result.Failure(FileStorageErrors.Create(FileStorageErrorCode.StorageUnavailable)));
        }
        catch (UnauthorizedAccessException)
        {
            return Task.FromResult(Result.Failure(FileStorageErrors.Create(FileStorageErrorCode.StorageUnavailable)));
        }
    }

    private async Task<long> CopyToTemporaryFileAsync(Stream content, string temporaryPath, CancellationToken cancellationToken)
    {
        await using var output = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var buffer = new byte[BufferSize];
        long totalBytes = 0;
        int bytesRead;
        while ((bytesRead = await content.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
        {
            totalBytes += bytesRead;
            if (totalBytes > maximumObjectSizeBytes)
            {
                throw new FileStorageSizeLimitExceededException();
            }

            await output.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
        }

        await output.FlushAsync(cancellationToken);
        return totalBytes;
    }

    private string ResolveContainedPath(string storageKey)
    {
        var relativePath = storageKey.Replace('/', Path.DirectorySeparatorChar);
        var resolvedPath = Path.GetFullPath(Path.Combine(rootPath, relativePath));
        var relativeToRoot = Path.GetRelativePath(rootPath, resolvedPath);
        if (Path.IsPathRooted(relativeToRoot) || relativeToRoot == ".." || relativeToRoot.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
        {
            throw new ArgumentException("Storage key resolves outside the configured root.", nameof(storageKey));
        }

        return resolvedPath;
    }

    private static void DeleteTemporaryFile(string temporaryPath)
    {
        try
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private sealed class FileStorageSizeLimitExceededException : Exception;
}
