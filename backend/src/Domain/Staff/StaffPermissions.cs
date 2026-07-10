namespace PlayerPerformance.Domain.Staff;

/// <summary>Explicit application permissions granted to a staff access profile.</summary>
public sealed record StaffPermissions(
    bool CanVerifyReports,
    bool CanImportData,
    bool CanViewMedicalDetails)
{
    public static StaffPermissions None { get; } = new(false, false, false);

    public static StaffPermissions All { get; } = new(true, true, true);
}
