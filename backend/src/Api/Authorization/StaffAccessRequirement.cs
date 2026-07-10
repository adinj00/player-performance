using Microsoft.AspNetCore.Authorization;
using PlayerPerformance.Application.Authorization;

namespace PlayerPerformance.Api.Authorization;

internal enum StaffAccessCapability
{
    AdminOnly,
    VerifyReports,
    ImportData,
    ViewMedicalDetails
}

internal sealed class StaffAccessRequirement(StaffAccessCapability capability) : IAuthorizationRequirement
{
    public StaffAccessCapability Capability { get; } = capability;
}

internal sealed class StaffAccessAuthorizationHandler(ICurrentUserAccess currentUserAccess)
    : AuthorizationHandler<StaffAccessRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StaffAccessRequirement requirement)
    {
        var access = await currentUserAccess.GetAsync();
        if (!access.IsAuthenticated || !access.IsActive || !access.HasAccessProfile)
        {
            return;
        }

        if (access.IsAdmin || requirement.Capability switch
        {
            StaffAccessCapability.VerifyReports => access.Permissions.CanVerifyReports,
            StaffAccessCapability.ImportData => access.Permissions.CanImportData,
            StaffAccessCapability.ViewMedicalDetails => access.Permissions.CanViewMedicalDetails,
            _ => false
        })
        {
            context.Succeed(requirement);
        }
    }
}
