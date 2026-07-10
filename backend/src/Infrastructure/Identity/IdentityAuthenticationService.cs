using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PlayerPerformance.Application.Auth;
using PlayerPerformance.Application.Authorization;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Users;

namespace PlayerPerformance.Infrastructure.Identity;

public sealed class IdentityAuthenticationService(
    IHttpContextAccessor httpContextAccessor,
    ApplicationSignInManager signInManager,
    UserManager<ApplicationUser> userManager,
    ICurrentUserAccess currentUserAccess) : IAuthenticationService
{
    public async Task<Result<SessionResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<SessionResponse>.Failure(AuthenticationErrors.InvalidRequest);
        }

        var user = await userManager.FindByEmailAsync(request.Email.Trim());

        if (user is null)
        {
            return Result<SessionResponse>.Failure(AuthenticationErrors.InvalidCredentials);
        }

        if (await IsUnavailableAsync(user))
        {
            return Result<SessionResponse>.Failure(AuthenticationErrors.AccountUnavailable);
        }

        var signInResult = await signInManager.PasswordSignInAsync(user, request.Password, false, true);

        if (signInResult.Succeeded)
        {
            return Result<SessionResponse>.Success(await ToSessionAsync(user, cancellationToken));
        }

        if (signInResult.IsLockedOut || await userManager.IsLockedOutAsync(user))
        {
            return Result<SessionResponse>.Failure(AuthenticationErrors.AccountUnavailable);
        }

        return Result<SessionResponse>.Failure(AuthenticationErrors.InvalidCredentials);
    }

    public async Task<Result<SessionResponse>> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return Result<SessionResponse>.Failure(AuthenticationErrors.InvalidRequest);
        }

        if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return Result<SessionResponse>.Failure(AuthenticationErrors.PasswordConfirmationMismatch);
        }

        var user = await GetCurrentUserAsync();

        if (user is null || await IsUnavailableAsync(user))
        {
            return Result<SessionResponse>.Failure(AuthenticationErrors.AccountUnavailable);
        }

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            return Result<SessionResponse>.Failure(result.Errors.Any(error => error.Code == "PasswordMismatch")
                ? AuthenticationErrors.CurrentPasswordInvalid
                : AuthenticationErrors.PasswordValidationFailed);
        }

        user.RequiresPasswordChange = false;
        user.UpdatedUtc = DateTimeOffset.UtcNow;
        var updateResult = await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return Result<SessionResponse>.Failure(AuthenticationErrors.PasswordChangeFailed);
        }

        await signInManager.RefreshSignInAsync(user);
        return Result<SessionResponse>.Success(await ToSessionAsync(user, cancellationToken));
    }

    public async Task<SessionResponse> GetCurrentSessionAsync(CancellationToken cancellationToken = default)
    {
        var user = await GetCurrentUserAsync();

        if (user is null || await IsUnavailableAsync(user))
        {
            if (user is not null)
            {
                await signInManager.SignOutAsync();
            }

            return SessionResponse.Unauthenticated();
        }

        return await ToSessionAsync(user, cancellationToken);
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default) => signInManager.SignOutAsync();

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var principal = httpContextAccessor.HttpContext?.User;
        return principal?.Identity?.IsAuthenticated == true
            ? await userManager.GetUserAsync(principal)
            : null;
    }

    private async Task<bool> IsUnavailableAsync(ApplicationUser user)
    {
        return user.AccountStatus is not UserAccountStatus.ACTIVE || await userManager.IsLockedOutAsync(user);
    }

    private async Task<SessionResponse> ToSessionAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var access = await currentUserAccess.GetForUserAsync(user.Id, cancellationToken);
        var permissions = access.EffectivePermissions;
        return SessionResponse.Authenticated(new SessionUser(
            user.Id.ToString(),
            user.Email ?? string.Empty,
            user.AccountStatus.ToString(),
            user.RequiresPasswordChange,
            access.PrimaryRole?.ToString(),
            new SessionPermissions(
                permissions.CanVerifyReports,
                permissions.CanImportData,
                permissions.CanViewMedicalDetails),
            new SessionTeamScope(
                access.IsAdmin ? "ALL_TEAMS" : access.TeamScopeType.ToString(),
                access.IsAdmin || access.TeamScopeType == Domain.Staff.TeamScopeType.ALL_TEAMS ? [] : access.SelectedTeamIds.Select(id => id.ToString()).ToList())));
    }
}
