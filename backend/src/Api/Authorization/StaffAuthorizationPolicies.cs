namespace PlayerPerformance.Api.Authorization;

public static class StaffAuthorizationPolicies
{
    public const string AdminOnly = nameof(AdminOnly);
    public const string CanVerifyReports = nameof(CanVerifyReports);
    public const string CanImportData = nameof(CanImportData);
    public const string CanViewMedicalDetails = nameof(CanViewMedicalDetails);
}
