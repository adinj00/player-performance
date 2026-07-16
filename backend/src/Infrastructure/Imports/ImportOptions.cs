namespace PlayerPerformance.Infrastructure.Imports;
public sealed class ImportOptions
{
    public const string SectionName = "Imports";
    public long MaxUploadSizeBytes { get; set; } = 64 * 1024 * 1024;
    public int PreviewRowLimit { get; set; } = 100;
    public int ProcessingLeaseTimeoutMinutes { get; set; } = 30;
    public int HeaderScanRowLimit { get; set; } = 25;
    public int CsvDetectionRowLimit { get; set; } = 25;
    public int MaxRowsPerFile { get; set; } = 100000;
    public int MaxColumnsPerFile { get; set; } = 500;
    public int MaxCellLengthCharacters { get; set; } = 10000;
    public int MaxXlsxEntryCount { get; set; } = 5000;
    public long MaxXlsxUncompressedSizeBytes { get; set; } = 536870912;
    public double MaxXlsxCompressionRatio { get; set; } = 100;
    public string TemporaryWorkingDirectory { get; set; } = Path.Combine(Path.GetTempPath(), "player-performance-imports");
}
