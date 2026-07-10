using PlayerPerformance.Application.Authorization;
using PlayerPerformance.Domain.Staff;

namespace PlayerPerformance.Infrastructure.Authorization;

internal sealed class TeamAccessService(ICurrentUserAccess currentUserAccess) : ITeamAccessService
{
    public async Task<bool> CanAccessAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        if (teamId == Guid.Empty)
            return false;
        var access = await currentUserAccess.GetAsync(cancellationToken);
        return access.IsActive && access.HasAccessProfile && (access.IsAdmin || access.TeamScopeType == TeamScopeType.ALL_TEAMS || access.SelectedTeamIds.Contains(teamId));
    }
}
