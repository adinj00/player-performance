using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;

namespace PlayerPerformance.Application.Auth;

/// <summary>Input used to authenticate a staff user.</summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>Input used by the authenticated user to change their own password.</summary>
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);

/// <summary>Safe authenticated-user information exposed to the client session.</summary>
public sealed record SessionUser(string Id, string Email, string AccountStatus, bool MustChangePassword);

/// <summary>Safe representation of the current authentication session.</summary>
public sealed record SessionResponse(bool IsAuthenticated, SessionUser? User)
{
    public static SessionResponse Unauthenticated() => new(false, null);

    public static SessionResponse Authenticated(SessionUser user) => new(true, user);
}

/// <summary>Authentication use cases implemented by the infrastructure identity adapter.</summary>
public interface IAuthenticationService
{
    Task<Result<SessionResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<Result<SessionResponse>> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<SessionResponse> GetCurrentSessionAsync(CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);
}

/// <summary>Stable error values returned by authentication use cases.</summary>
public static class AuthenticationErrors
{
    public static readonly Error InvalidRequest = new("invalid_request", "The request is incomplete or invalid.");
    public static readonly Error InvalidCredentials = new("invalid_credentials", "Invalid email or password.");
    public static readonly Error AccountUnavailable = new("account_unavailable", "This account cannot sign in.");
    public static readonly Error CurrentPasswordInvalid = new("current_password_invalid", "The current password is incorrect.");
    public static readonly Error PasswordConfirmationMismatch = new("password_confirmation_mismatch", "The new password confirmation does not match.");
    public static readonly Error PasswordValidationFailed = new("password_validation_failed", "The new password does not meet the configured requirements.");
    public static readonly Error PasswordChangeFailed = new("password_change_failed", "The password could not be changed.");
}
