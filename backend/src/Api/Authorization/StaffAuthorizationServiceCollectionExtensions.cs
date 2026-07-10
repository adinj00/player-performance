using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace PlayerPerformance.Api.Authorization;

internal static class StaffAuthorizationServiceCollectionExtensions
{
    public static IServiceCollection AddStaffAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(StaffAuthorizationPolicies.AdminOnly, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new StaffAccessRequirement(StaffAccessCapability.AdminOnly)));
            options.AddPolicy(StaffAuthorizationPolicies.CanVerifyReports, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new StaffAccessRequirement(StaffAccessCapability.VerifyReports)));
            options.AddPolicy(StaffAuthorizationPolicies.CanImportData, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new StaffAccessRequirement(StaffAccessCapability.ImportData)));
            options.AddPolicy(StaffAuthorizationPolicies.CanViewMedicalDetails, policy =>
                policy.RequireAuthenticatedUser().AddRequirements(new StaffAccessRequirement(StaffAccessCapability.ViewMedicalDetails)));
        });
        services.AddScoped<IAuthorizationHandler, StaffAccessAuthorizationHandler>();
        return services;
    }
}
