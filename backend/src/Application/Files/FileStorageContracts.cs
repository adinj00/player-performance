using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;

namespace PlayerPerformance.Application.Files;

/// <summary>
/// Provider-neutral storage for opaque, server-generated file objects.
/// Callers retain ownership of write streams and must dispose streams returned by reads.
/// </summary>
public interface IFileStorage
{
    Task<Result<FileStorageWriteResult>> WriteAsync(FileStorageWriteRequest request, CancellationToken cancellationToken);

    Task<Result<Stream>> OpenReadAsync(string storageKey, CancellationToken cancellationToken);

    Task<Result> DeleteIfExistsAsync(string storageKey, CancellationToken cancellationToken);
}

public sealed record FileStorageWriteRequest(Stream Content, string StorageKey, long? DeclaredLength);

public sealed record FileStorageWriteResult(string StorageKey, long SizeBytes);

public enum FileStorageErrorCode
{
    InvalidStorageKey,
    EmptyContent,
    ObjectTooLarge,
    ObjectAlreadyExists,
    ObjectNotFound,
    StorageUnavailable,
    OperationCancelled
}

public static class FileStorageErrors
{
    public static Error Create(FileStorageErrorCode code) => new(
        $"file_storage.{code.ToString().ToLowerInvariant()}",
        code switch
        {
            FileStorageErrorCode.InvalidStorageKey => "The storage key is invalid.",
            FileStorageErrorCode.EmptyContent => "File content cannot be empty.",
            FileStorageErrorCode.ObjectTooLarge => "File content exceeds the allowed storage limit.",
            FileStorageErrorCode.ObjectAlreadyExists => "A stored object already exists for this key.",
            FileStorageErrorCode.ObjectNotFound => "The stored object was not found.",
            FileStorageErrorCode.StorageUnavailable => "File storage is currently unavailable.",
            FileStorageErrorCode.OperationCancelled => "The storage operation was cancelled.",
            _ => "The storage operation failed."
        });
}
