using PlayerPerformance.Domain.Staff;

namespace PlayerPerformance.Infrastructure.Staff;

public sealed class StaffAccessProfile
{
    public Guid UserId {
        get; set;
    }

    public StaffRole PrimaryRole {
        get; set;
    }
    public string DisplayName { get; set; } = string.Empty;
    public TeamScopeType TeamScopeType { get; set; } = TeamScopeType.ALL_TEAMS;

    public bool CanVerifyReports {
        get; set;
    }

    public bool CanImportData {
        get; set;
    }

    public bool CanViewMedicalDetails {
        get; set;
    }

    public DateTimeOffset CreatedUtc {
        get; set;
    }

    public DateTimeOffset? UpdatedUtc {
        get; set;
    }
}
