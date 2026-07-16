namespace PlayerPerformance.Infrastructure.Imports;
public sealed class ImportOptions
{
    public const string SectionName = "Imports";
    public long MaxUploadSizeBytes { get; set; } = 64 * 1024 * 1024;
    public int PreviewRowLimit { get; set; } = 100;
    public int ProcessingLeaseTimeoutMinutes { get; set; } = 30;
}
