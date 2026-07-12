using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;

namespace PlayerPerformance.Application.Files;

public static class FileMetadataValidation
{
    public const int OriginalFileNameMaxLength = 255;
    public const int ContentTypeMaxLength = 255;

    public static Result<string> NormalizeOriginalFileName(string? originalFileName)
    {
        var finalComponent = originalFileName?.Replace('\\', '/').Split('/').LastOrDefault()?.Trim();
        if (string.IsNullOrWhiteSpace(finalComponent) || finalComponent.Length > OriginalFileNameMaxLength || finalComponent.Any(char.IsControl))
        {
            return Result<string>.Failure(new Error("file_metadata.invalid_original_file_name", "The original file name is invalid."));
        }

        return Result<string>.Success(finalComponent);
    }

    public static bool IsValidContentType(string? contentType) =>
        !string.IsNullOrWhiteSpace(contentType)
        && contentType.Length <= ContentTypeMaxLength
        && !contentType.Any(char.IsControl);
}
