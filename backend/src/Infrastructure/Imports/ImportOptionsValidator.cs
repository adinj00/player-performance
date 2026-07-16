using Microsoft.Extensions.Options;
using PlayerPerformance.Infrastructure.Files;
namespace PlayerPerformance.Infrastructure.Imports;

public sealed class ImportOptionsValidator(IOptions<FileStorageOptions> storage) : IValidateOptions<ImportOptions>
{
    public ValidateOptionsResult Validate(string? name, ImportOptions options) => options.MaxUploadSizeBytes <= 0 || options.MaxUploadSizeBytes > storage.Value.MaxObjectSizeBytes ? ValidateOptionsResult.Fail("The import upload maximum must be positive and cannot exceed the storage maximum.") : options.PreviewRowLimit is < 1 or > 1000 ? ValidateOptionsResult.Fail("The import preview row limit must be between 1 and 1000.") : options.ProcessingLeaseTimeoutMinutes is < 1 or > 1440 ? ValidateOptionsResult.Fail("The import processing lease timeout must be between 1 and 1440 minutes.") : ValidateOptionsResult.Success;
}
