namespace PlayerPerformance.Api.Configuration;

public sealed class PlayerPerformanceOptions
{
    public const string SectionName = "PlayerPerformance";

    public string ServiceName { get; init; } = string.Empty;

    public string? FrontendOrigin
    {
        get; init;
    }

    public static bool IsValidFrontendOrigin(string? frontendOrigin)
    {
        if (string.IsNullOrWhiteSpace(frontendOrigin))
        {
            return true;
        }

        return Uri.TryCreate(frontendOrigin, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
