using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Staff;

namespace PlayerPerformance.Application.Users;

public sealed record CreateStaffInvitationRequest(string? DisplayName, string? Email, StaffRole PrimaryRole, bool? CanVerifyReports, bool? CanImportData, bool? CanViewMedicalDetails, TeamScopeType TeamScopeType, IReadOnlyList<Guid>? SelectedTeamIds);
public sealed record UpdateStaffProfileRequest(string? DisplayName);
public sealed record ReplaceStaffAccessRequest(StaffRole PrimaryRole, bool? CanVerifyReports, bool? CanImportData, bool? CanViewMedicalDetails, TeamScopeType TeamScopeType, IReadOnlyList<Guid>? SelectedTeamIds);
public sealed record AcceptStaffInvitationRequest(string? Email, string? Token, string? Password, string? ConfirmPassword);
public sealed record StaffPermissionsResponse(bool CanVerifyReports, bool CanImportData, bool CanViewMedicalDetails);
public sealed record StaffTeamScopeResponse(TeamScopeType Type, IReadOnlyList<Guid> SelectedTeamIds);
public sealed record StaffUserResponse(Guid Id, string DisplayName, string Email, string Status, StaffRole PrimaryRole, StaffPermissionsResponse Permissions, StaffTeamScopeResponse TeamScope, DateTimeOffset CreatedAtUtc, DateTimeOffset? UpdatedAtUtc);
public sealed record StaffInvitationCredentialResponse(StaffUserResponse User, string SetupToken);

public interface IStaffUsersService
{
    Task<IReadOnlyList<StaffUserResponse>> ListAsync(string? search, StaffRole? role, string? status, TeamScopeType? scopeType, Guid? teamId, CancellationToken ct);
    Task<StaffUserResponse?> GetAsync(Guid userId, CancellationToken ct);
    Task<Result<StaffInvitationCredentialResponse>> CreateInvitationAsync(CreateStaffInvitationRequest request, CancellationToken ct);
    Task<Result<StaffInvitationCredentialResponse>> ReissueInvitationAsync(Guid userId, CancellationToken ct);
    Task<Result<StaffUserResponse>> UpdateProfileAsync(Guid userId, UpdateStaffProfileRequest request, CancellationToken ct);
    Task<Result<StaffUserResponse>> ReplaceAccessAsync(Guid userId, ReplaceStaffAccessRequest request, CancellationToken ct);
    Task<Result<StaffUserResponse>> DisableAsync(Guid userId, CancellationToken ct);
    Task<Result<StaffUserResponse>> ReactivateAsync(Guid userId, CancellationToken ct);
    Task<Result> AcceptInvitationAsync(AcceptStaffInvitationRequest request, CancellationToken ct);
}

public static class StaffUserErrors
{
    public static readonly Error Validation = new("validation_failed", "One or more fields are invalid.");
    public static readonly Error NotFound = new("not_found", "The requested staff user was not found.");
    public static readonly Error Conflict = new("staff_conflict", "The requested account change conflicts with its current state.");
    public static readonly Error DuplicateEmail = new("duplicate_email", "A staff account with that email already exists.");
    public static readonly Error InvalidInvitation = new("invalid_invitation", "The invitation credentials are invalid or expired.");
}
