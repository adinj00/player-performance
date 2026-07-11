using Microsoft.EntityFrameworkCore;
using PlayerPerformance.Application.Players;
using PlayerPerformance.Domain.Players;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.Infrastructure.Players;

internal sealed class PlayerTeamAssignmentsRepository(AppDbContext dbContext) : IPlayerTeamAssignmentsRepository
{
    public Task<Player?> GetPlayerAsync(Guid playerId, CancellationToken ct) => dbContext.Players.SingleOrDefaultAsync(x => x.Id == playerId, ct);
    public Task<Team?> GetTeamAsync(Guid teamId, CancellationToken ct) => dbContext.Teams.SingleOrDefaultAsync(x => x.Id == teamId, ct);
    public Task<PlayerTeamAssignment?> GetAssignmentAsync(Guid assignmentId, CancellationToken ct) => dbContext.PlayerTeamAssignments.SingleOrDefaultAsync(x => x.Id == assignmentId, ct);
    public Task<bool> HasCurrentAssignmentsAsync(Guid playerId, DateOnly today, CancellationToken ct) => dbContext.PlayerTeamAssignments.AnyAsync(x => x.PlayerId == playerId && x.StartDate <= today && (x.EndDate == null || x.EndDate >= today), ct);
    public Task<bool> HasOverlapAsync(Guid playerId, Guid teamId, DateOnly startDate, DateOnly? endDate, Guid? excludingAssignmentId, CancellationToken ct) => dbContext.PlayerTeamAssignments.AnyAsync(x => x.PlayerId == playerId && x.TeamId == teamId && (!excludingAssignmentId.HasValue || x.Id != excludingAssignmentId.Value) && x.StartDate <= (endDate ?? DateOnly.MaxValue) && (x.EndDate == null || x.EndDate >= startDate), ct);
    public async Task<IReadOnlyList<PlayerTeamAssignmentReadModel>> ListAsync(Guid playerId, CancellationToken ct) => await (from assignment in dbContext.PlayerTeamAssignments.AsNoTracking()
                                                                                                                             join team in dbContext.Teams.AsNoTracking() on assignment.TeamId equals team.Id
                                                                                                                             where assignment.PlayerId == playerId
                                                                                                                             select new PlayerTeamAssignmentReadModel(assignment, team.Name)).ToListAsync(ct);
    public void Add(PlayerTeamAssignment assignment) => dbContext.PlayerTeamAssignments.Add(assignment);
    public Task SaveChangesAsync(CancellationToken ct) => dbContext.SaveChangesAsync(ct);
}
