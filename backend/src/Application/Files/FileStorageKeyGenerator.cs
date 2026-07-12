using PlayerPerformance.Application.Abstractions.Time;

namespace PlayerPerformance.Application.Files;

public interface IFileStorageKeyGenerator
{
    string Create();
}

public sealed class FileStorageKeyGenerator(ISystemClock clock) : IFileStorageKeyGenerator
{
    public string Create()
    {
        var now = clock.UtcNow;
        return $"objects/{now:yyyy}/{now:MM}/{Guid.NewGuid():N}";
    }
}

public static class FileStorageKeyValidator
{
    public static bool IsValid(string? storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || storageKey.StartsWith("/", StringComparison.Ordinal) || storageKey.Contains('\\'))
        {
            return false;
        }

        if (Path.IsPathRooted(storageKey) || storageKey.Contains("://", StringComparison.Ordinal) || storageKey.Contains(':'))
        {
            return false;
        }

        var segments = storageKey.Split('/', StringSplitOptions.None);
        return segments.Length > 0 && segments.All(segment => !string.IsNullOrWhiteSpace(segment) && segment != "." && segment != "..");
    }
}
