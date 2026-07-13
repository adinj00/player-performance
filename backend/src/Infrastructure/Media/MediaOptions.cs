using Microsoft.Extensions.Options;
using PlayerPerformance.Infrastructure.Files;
namespace PlayerPerformance.Infrastructure.Media;

public sealed class MediaOptions
{
    public const string SectionName = "Media";
    public long MaxUploadSizeBytes { get; set; } = 536_870_912;
}
internal sealed class MediaOptionsValidator(IOptions<FileStorageOptions> storage) : IValidateOptions<MediaOptions>
{
    public ValidateOptionsResult Validate(string? name, MediaOptions options) => options.MaxUploadSizeBytes <= 0 ? ValidateOptionsResult.Fail("The media upload maximum must be positive.") : options.MaxUploadSizeBytes > storage.Value.MaxObjectSizeBytes ? ValidateOptionsResult.Fail("The media upload maximum cannot exceed the storage maximum.") : ValidateOptionsResult.Success;
}
