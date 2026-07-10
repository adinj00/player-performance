namespace PlayerPerformance.Api.Authentication;

internal static class ApiAntiforgeryConstants
{
    public const string HeaderName = "X-CSRF-TOKEN";
    public const string CookieName = "XSRF-TOKEN";
}
