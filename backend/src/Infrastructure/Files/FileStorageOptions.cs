namespace PlayerPerformance.Infrastructure.Files;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string? Provider { get; set; }
    public string? LocalRootPath { get; set; }
    public long MaxObjectSizeBytes { get; set; }
    public string? ResolvedLocalRootPath { get; set; }
}
