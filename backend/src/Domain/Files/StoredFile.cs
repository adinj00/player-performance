using PlayerPerformance.Domain.Common.Entities;
using PlayerPerformance.Domain.Common.Guards;

namespace PlayerPerformance.Domain.Files;

public sealed class StoredFile : Entity
{
    public const int OriginalFileNameMaxLength = 255;
    public const int ContentTypeMaxLength = 255;

    private StoredFile() : base(Guid.Empty)
    {
        StorageKey = string.Empty;
        OriginalFileName = string.Empty;
        ContentType = string.Empty;
    }

    private StoredFile(Guid id, string storageKey, string originalFileName, string contentType, long sizeBytes, Guid uploadedByUserId, DateTime createdAtUtc) : base(id)
    {
        StorageKey = storageKey;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        UploadedByUserId = uploadedByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    public string StorageKey { get; private set; }
    public string OriginalFileName { get; private set; }
    public string ContentType { get; private set; }
    public long SizeBytes { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime? ArchivedAtUtc { get; private set; }
    public Guid? ArchivedByUserId { get; private set; }

    public static StoredFile Create(Guid id, string storageKey, string originalFileName, string contentType, long sizeBytes, Guid uploadedByUserId, DateTime createdAtUtc)
    {
        Guard.AgainstDefault(id, nameof(id));
        Guard.AgainstDefault(uploadedByUserId, nameof(uploadedByUserId));
        EnsureRequired(storageKey, nameof(storageKey), 512);
        EnsureStorageKey(storageKey);
        EnsureRequired(originalFileName, nameof(originalFileName), OriginalFileNameMaxLength);
        EnsureRequired(contentType, nameof(contentType), ContentTypeMaxLength);
        if (sizeBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeBytes));
        }

        return new(id, storageKey, originalFileName, contentType, sizeBytes, uploadedByUserId, createdAtUtc);
    }

    public void Archive(Guid archivedByUserId, DateTime archivedAtUtc)
    {
        Guard.AgainstDefault(archivedByUserId, nameof(archivedByUserId));
        if (IsArchived)
        {
            return;
        }

        IsArchived = true;
        ArchivedByUserId = archivedByUserId;
        ArchivedAtUtc = archivedAtUtc;
    }

    private static void EnsureRequired(string? value, string parameterName, int maximumLength)
    {
        Guard.AgainstNullOrWhiteSpace(value, parameterName);
        if (value!.Length > maximumLength || value.Any(char.IsControl))
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }

    private static void EnsureStorageKey(string storageKey)
    {
        if (storageKey.StartsWith("/", StringComparison.Ordinal)
            || storageKey.Contains('\\')
            || storageKey.Contains("..", StringComparison.Ordinal)
            || storageKey.Contains(':'))
        {
            throw new ArgumentOutOfRangeException(nameof(storageKey));
        }
    }
}
