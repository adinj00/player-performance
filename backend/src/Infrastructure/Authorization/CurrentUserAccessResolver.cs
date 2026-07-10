using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Application.Authorization;
using PlayerPerformance.Domain.Staff;
using PlayerPerformance.Domain.Users;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.Infrastructure.Authorization;

public sealed class CurrentUserAccessResolver(
    IHttpContextAccessor httpContextAccessor,
    AppDbContext dbContext,
    ISystemClock clock,
    ILogger<CurrentUserAccessResolver> logger) : ICurrentUserAccess
{
    private Task<CurrentUserAccess>? cachedAccess;

    public Task<CurrentUserAccess> GetAsync(CancellationToken cancellationToken = default) =>
        cachedAccess ??= ResolveAsync(cancellationToken);

    public Task<CurrentUserAccess> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        LoadForUserAsync(userId, cancellationToken);

    private async Task<CurrentUserAccess> ResolveAsync(CancellationToken cancellationToken)
    {
        var principal = httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return CurrentUserAccess.Unauthenticated();
        }

        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            logger.LogWarning("Authenticated request did not contain a valid staff user identifier.");
            return CurrentUserAccess.Unauthenticated();
        }

        return await LoadForUserAsync(userId, cancellationToken);
    }

    private async Task<CurrentUserAccess> LoadForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.AsNoTracking()
            .Where(candidate => candidate.Id == userId)
            .Select(candidate => new { candidate.AccountStatus, candidate.LockoutEnd })
            .SingleOrDefaultAsync(cancellationToken);

        var isActive = user is not null
            && user.AccountStatus == UserAccountStatus.ACTIVE
            && (user.LockoutEnd is null || user.LockoutEnd <= clock.UtcNow);
        if (!isActive)
        {
            logger.LogWarning("Authenticated staff user is unavailable or no longer exists.");
            return new CurrentUserAccess(userId, true, false, null, StaffPermissions.None);
        }

        var profile = await dbContext.StaffAccessProfiles.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);
        if (profile is null)
        {
            logger.LogWarning("Authenticated active staff user has no access profile.");
            return new CurrentUserAccess(userId, true, true, null, StaffPermissions.None);
        }

        return new CurrentUserAccess(
            userId,
            true,
            true,
            profile.PrimaryRole,
            new StaffPermissions(profile.CanVerifyReports, profile.CanImportData, profile.CanViewMedicalDetails));
    }
}
