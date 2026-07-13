using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace PlayerPerformance.Infrastructure.Files;

public sealed class FileStorageOptionsValidator(IHostEnvironment environment) : IValidateOptions<FileStorageOptions>
{
    public ValidateOptionsResult Validate(string? name, FileStorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Provider))
        {
            return ValidateOptionsResult.Fail("File storage provider is required.");
        }

        if (!string.Equals(options.Provider, "Local", StringComparison.OrdinalIgnoreCase))
        {
            return ValidateOptionsResult.Fail("The configured file storage provider is not supported.");
        }

        if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing"))
        {
            return ValidateOptionsResult.Fail("The Local file storage provider is allowed only in the Development environment.");
        }

        if (string.IsNullOrWhiteSpace(options.LocalRootPath))
        {
            return ValidateOptionsResult.Fail("A local file storage root is required.");
        }

        if (options.MaxObjectSizeBytes <= 0)
        {
            return ValidateOptionsResult.Fail("The file storage maximum object size must be positive.");
        }

        try
        {
            var resolvedRoot = Path.GetFullPath(options.LocalRootPath, environment.ContentRootPath);
            var webRoot = Path.GetFullPath("wwwroot", environment.ContentRootPath);
            if (IsSameOrChildPath(resolvedRoot, webRoot) || IsFrontendPublicDirectory(resolvedRoot))
            {
                return ValidateOptionsResult.Fail("The local file storage root cannot be a publicly served directory.");
            }
            Directory.CreateDirectory(resolvedRoot);
            options.ResolvedLocalRootPath = resolvedRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return ValidateOptionsResult.Success;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or NotSupportedException or UnauthorizedAccessException)
        {
            return ValidateOptionsResult.Fail("The local file storage root cannot be created or accessed.");
        }
    }

    private static bool IsSameOrChildPath(string candidate, string root)
    {
        var relative = Path.GetRelativePath(root, candidate);
        return relative == "." || (!Path.IsPathRooted(relative) && relative != ".." && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
    }

    private static bool IsFrontendPublicDirectory(string path)
    {
        var segments = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return segments.Length >= 2
            && string.Equals(segments[^2], "frontend", StringComparison.OrdinalIgnoreCase)
            && string.Equals(segments[^1], "public", StringComparison.OrdinalIgnoreCase);
    }
}
