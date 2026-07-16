using Microsoft.Extensions.Options;
using PlayerPerformance.Infrastructure.Files;
namespace PlayerPerformance.Infrastructure.Imports;

public sealed class ImportOptionsValidator(IOptions<FileStorageOptions> storage) : IValidateOptions<ImportOptions>
{
    public ValidateOptionsResult Validate(string? name, ImportOptions options)
    {
        if (options.MaxUploadSizeBytes <= 0 || options.MaxUploadSizeBytes > storage.Value.MaxObjectSizeBytes)
            return ValidateOptionsResult.Fail("The import upload maximum must be positive and cannot exceed the storage maximum.");
        if (options.PreviewRowLimit is < 1 or > 1000 || options.ProcessingLeaseTimeoutMinutes is < 1 or > 1440 || options.HeaderScanRowLimit is < 1 or > 1000 || options.CsvDetectionRowLimit is < 1 or > 1000 || options.MaxRowsPerFile is < 1 or > 1000000 || options.MaxColumnsPerFile is < 1 or > 10000 || options.MaxCellLengthCharacters is < 1 or > 1000000 || options.MaxXlsxEntryCount is < 1 or > 100000 || options.MaxXlsxUncompressedSizeBytes <= 0 || options.MaxXlsxUncompressedSizeBytes > storage.Value.MaxObjectSizeBytes * 20 || options.MaxXlsxCompressionRatio is < 1 or > 10000)
            return ValidateOptionsResult.Fail("One or more import parser safety limits are invalid.");
        if (string.IsNullOrWhiteSpace(options.TemporaryWorkingDirectory) || !Path.IsPathFullyQualified(Path.GetFullPath(options.TemporaryWorkingDirectory)))
            return ValidateOptionsResult.Fail("The import temporary working directory is invalid.");
        return ValidateOptionsResult.Success;
    }
}
