using PlayerPerformance.Domain.Common.Results;

namespace PlayerPerformance.Application.Files;

/// <summary>
/// Supports the required storage compensation step after a higher-level metadata transaction fails.
/// A failed delete is returned to the caller so it can be logged with the operation correlation ID.
/// </summary>
public static class FileStorageCompensation
{
    public static Task<Result> DeleteAfterPersistenceFailureAsync(
        IFileStorage fileStorage,
        string storageKey,
        CancellationToken cancellationToken) =>
        fileStorage.DeleteIfExistsAsync(storageKey, cancellationToken);
}
