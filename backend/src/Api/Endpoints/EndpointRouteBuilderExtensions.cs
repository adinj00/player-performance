namespace PlayerPerformance.Api.Endpoints;

using PlayerPerformance.Api.Authorization;
using PlayerPerformance.Api.Endpoints.Settings;

internal static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder endpoints, IHostEnvironment environment)
    {
        endpoints.MapHealthEndpoints();
        endpoints.MapAuthEndpoints();
        endpoints.MapSettingsEndpoints();

        if (environment.IsEnvironment("Testing"))
        {
            endpoints.MapGet("/_test/protected", () => TypedResults.NoContent())
                .RequireAuthorization();
            endpoints.MapGet("/_test/policies/admin", () => TypedResults.NoContent())
                .RequireAuthorization(StaffAuthorizationPolicies.AdminOnly);
            endpoints.MapGet("/_test/policies/verify-reports", () => TypedResults.NoContent())
                .RequireAuthorization(StaffAuthorizationPolicies.CanVerifyReports);
            endpoints.MapGet("/_test/policies/import-data", () => TypedResults.NoContent())
                .RequireAuthorization(StaffAuthorizationPolicies.CanImportData);
            endpoints.MapGet("/_test/policies/medical-details", () => TypedResults.NoContent())
                .RequireAuthorization(StaffAuthorizationPolicies.CanViewMedicalDetails);
        }

        return endpoints;
    }
}
