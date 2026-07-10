using PlayerPerformance.Domain.Staff;

namespace PlayerPerformance.Application.Authorization;

/// <summary>Resolves the persisted access data for the current request user.</summary>
public interface ICurrentUserAccess
{
    Task<CurrentUserAccess> GetAsync(CancellationToken cancellationToken = default);

    Task<CurrentUserAccess> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>Application-safe representation of the authenticated user's current access.</summary>
public sealed record CurrentUserAccess(
    Guid? UserId,
    bool IsAuthenticated,
    bool IsActive,
    StaffRole? PrimaryRole,
    StaffPermissions Permissions)
{
    public bool HasAccessProfile => PrimaryRole is not null;

    public bool IsAdmin => IsActive && PrimaryRole is StaffRole.ADMIN;

    public StaffPermissions EffectivePermissions => IsAdmin ? StaffPermissions.All : Permissions;

    public static CurrentUserAccess Unauthenticated() => new(null, false, false, null, StaffPermissions.None);
}
