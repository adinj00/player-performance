namespace PlayerPerformance.Application.Authorization;

/// <summary>Applies the effective staff team scope to a requested team.</summary>
public interface ITeamAccessService
{
    Task<bool> CanAccessAsync(Guid teamId, CancellationToken cancellationToken = default);
}
