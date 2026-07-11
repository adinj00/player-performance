using Microsoft.EntityFrameworkCore;
using PlayerPerformance.Application.Players;
using PlayerPerformance.Domain.Players;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.Infrastructure.Players;

internal sealed class PlayersRepository(AppDbContext dbContext) : IPlayersRepository
{
    public async Task<PagedPlayers> ListAsync(PlayerListQuery query, DateOnly today, IReadOnlyList<Guid>? selectedTeamIds, CancellationToken ct)
    {
        IQueryable<Player> players = dbContext.Players.AsNoTracking();
        var currentAssignments = dbContext.PlayerTeamAssignments.AsNoTracking().Where(x => x.StartDate <= today && (x.EndDate == null || x.EndDate >= today));
        if (selectedTeamIds is not null)
            players = players.Where(player => currentAssignments.Any(assignment => assignment.PlayerId == player.Id && selectedTeamIds.Contains(assignment.TeamId)));
        if (query.TeamId is { } teamId)
            players = players.Where(player => currentAssignments.Any(assignment => assignment.PlayerId == player.Id && assignment.TeamId == teamId));
        if (!query.IncludeArchived)
            players = players.Where(x => x.Status != PlayerRecordStatus.ARCHIVED);
        if (query.Status is { } status)
            players = players.Where(x => x.Status == status);
        if (query.Search is { Length: > 0 } search)
        {
            var normalizedSearch = search.ToUpperInvariant();
            players = players.Where(x => x.FirstName.ToUpper().Contains(normalizedSearch) || x.LastName.ToUpper().Contains(normalizedSearch) || (x.PreferredName != null && x.PreferredName.ToUpper().Contains(normalizedSearch)) || (x.FirstName + " " + x.LastName).ToUpper().Contains(normalizedSearch));
        }

        var totalCount = await players.CountAsync(ct);
        var page = await players.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ThenBy(x => x.Id).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        var ids = page.Select(x => x.Id).ToList();
        var summaries = await GetCurrentAssignmentsForPlayersAsync(ids, today, ct);
        return new PagedPlayers(page.Select(player => new PlayerReadModel(player, summaries.TryGetValue(player.Id, out var assignments) ? assignments : [])).ToList(), totalCount);
    }

    public Task<Player?> GetAsync(Guid id, CancellationToken ct) => dbContext.Players.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<PlayerReadModel?> GetReadAsync(Guid id, DateOnly today, IReadOnlyList<Guid>? selectedTeamIds, CancellationToken ct)
    {
        var player = await dbContext.Players.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (player is null)
            return null;
        var assignments = await GetCurrentAssignmentsAsync(id, today, ct);
        if (selectedTeamIds is not null && !assignments.Any(x => selectedTeamIds.Contains(x.TeamId)))
            return null;
        return new PlayerReadModel(player, assignments);
    }
    public async Task<IReadOnlyList<CurrentPlayerTeamAssignment>> GetCurrentAssignmentsAsync(Guid playerId, DateOnly today, CancellationToken ct) => (await GetCurrentAssignmentsForPlayersAsync([playerId], today, ct)).GetValueOrDefault(playerId, []);

    private async Task<Dictionary<Guid, IReadOnlyList<CurrentPlayerTeamAssignment>>> GetCurrentAssignmentsForPlayersAsync(IReadOnlyList<Guid> playerIds, DateOnly today, CancellationToken ct)
    {
        if (playerIds.Count == 0)
            return [];
        var assignments = await (from assignment in dbContext.PlayerTeamAssignments.AsNoTracking()
                                 join team in dbContext.Teams.AsNoTracking() on assignment.TeamId equals team.Id
                                 where playerIds.Contains(assignment.PlayerId) && assignment.StartDate <= today && (assignment.EndDate == null || assignment.EndDate >= today)
                                 orderby assignment.StartDate descending, assignment.EndDate descending, assignment.Id
                                 select new { assignment.PlayerId, assignment.TeamId, team.Name, assignment.StartDate, assignment.EndDate })
            .ToListAsync(ct);
        return assignments.GroupBy(x => x.PlayerId).ToDictionary(x => x.Key, x => (IReadOnlyList<CurrentPlayerTeamAssignment>) x.Select(a => new CurrentPlayerTeamAssignment(a.TeamId, a.Name, a.StartDate, a.EndDate)).ToList());
    }
    public void Add(Player player) => dbContext.Players.Add(player);
    public Task SaveChangesAsync(CancellationToken ct) => dbContext.SaveChangesAsync(ct);
}
