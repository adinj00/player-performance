using Microsoft.EntityFrameworkCore;
using PlayerPerformance.Application.Matches;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Domain.Players;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.Infrastructure.Matches;

internal sealed class MatchLineupRepository(AppDbContext dbContext) : IMatchLineupRepository
{
    public async Task<MatchLineupAggregate?> GetAsync(Guid matchId, CancellationToken ct)
    {
        var match = await dbContext.Matches.SingleOrDefaultAsync(x => x.Id == matchId, ct);
        if (match is null)
            return null;

        var lineup = await dbContext.MatchLineups.SingleOrDefaultAsync(x => x.MatchId == matchId, ct);
        var entries = await dbContext.MatchLineupEntries.Where(x => x.MatchId == matchId).ToListAsync(ct);
        var appearances = await dbContext.PlayerMatchAppearances.Where(x => x.MatchId == matchId).ToListAsync(ct);
        var substitutions = await dbContext.MatchSubstitutions.Where(x => x.MatchId == matchId).OrderBy(x => x.Sequence).ToListAsync(ct);
        return new(match, lineup, entries, appearances, substitutions);
    }

    public async Task<MatchLineupReadModel?> GetReadAsync(Guid matchId, IReadOnlyCollection<Guid>? teamScope, CancellationToken ct)
    {
        var match = await dbContext.Matches.AsNoTracking().SingleOrDefaultAsync(x => x.Id == matchId && (teamScope == null || teamScope.Contains(x.TeamId)), ct);
        if (match is null)
            return null;

        var lineup = await dbContext.MatchLineups.AsNoTracking().SingleOrDefaultAsync(x => x.MatchId == matchId, ct);
        var entries = await (from entry in dbContext.MatchLineupEntries.AsNoTracking()
                             join player in dbContext.Players.AsNoTracking() on entry.PlayerId equals player.Id
                             where entry.MatchId == matchId
                             orderby entry.Role, player.LastName, player.FirstName, player.Id
                             select new MatchLineupEntryReadModel(entry, player.FirstName, player.LastName, player.PreferredName)).ToListAsync(ct);
        var appearances = await dbContext.PlayerMatchAppearances.AsNoTracking().Where(x => x.MatchId == matchId).OrderBy(x => x.PlayerId).Select(x => new MatchAppearanceReadModel(x)).ToListAsync(ct);
        var substitutions = await dbContext.MatchSubstitutions.AsNoTracking().Where(x => x.MatchId == matchId).OrderBy(x => x.Sequence).ToListAsync(ct);
        return new(match, lineup, entries, appearances, substitutions);
    }

    public async Task<IReadOnlyDictionary<Guid, bool>> GetNewPlayerEligibilityAsync(IReadOnlyCollection<Guid> playerIds, Guid teamId, DateOnly matchDate, CancellationToken ct)
    {
        var ids = playerIds.Distinct().ToArray();
        var eligible = await (from player in dbContext.Players.AsNoTracking()
                              where ids.Contains(player.Id) && player.Status != PlayerRecordStatus.ARCHIVED
                              join assignment in dbContext.PlayerTeamAssignments.AsNoTracking() on player.Id equals assignment.PlayerId
                              where assignment.TeamId == teamId && assignment.StartDate <= matchDate && (assignment.EndDate == null || assignment.EndDate >= matchDate)
                              select player.Id).Distinct().ToListAsync(ct);
        return ids.ToDictionary(x => x, x => eligible.Contains(x));
    }

    public async Task<IReadOnlyList<EligibleLineupPlayerResponse>> GetEligiblePlayersAsync(Guid teamId, DateOnly matchDate, CancellationToken ct)
    {
        return await (from player in dbContext.Players.AsNoTracking()
                      join assignment in dbContext.PlayerTeamAssignments.AsNoTracking() on player.Id equals assignment.PlayerId
                      where player.Status != PlayerRecordStatus.ARCHIVED
                            && assignment.TeamId == teamId
                            && assignment.StartDate <= matchDate
                            && (assignment.EndDate == null || assignment.EndDate >= matchDate)
                      orderby player.LastName, player.FirstName, player.Id
                      select new EligibleLineupPlayerResponse(
                          player.Id,
                          player.FirstName,
                          player.LastName,
                          player.PreferredName,
                          player.Status,
                          assignment.StartDate,
                          assignment.EndDate))
            .ToListAsync(ct);
    }

    public void Add(MatchLineup lineup) => dbContext.MatchLineups.Add(lineup);
    public void AddEntry(MatchLineupEntry entry) => dbContext.MatchLineupEntries.Add(entry);
    public void AddAppearance(PlayerMatchAppearance appearance) => dbContext.PlayerMatchAppearances.Add(appearance);
    public void AddSubstitution(MatchSubstitution substitution) => dbContext.MatchSubstitutions.Add(substitution);
    public void RemoveEntry(MatchLineupEntry entry) => dbContext.MatchLineupEntries.Remove(entry);
    public void RemoveAppearance(PlayerMatchAppearance appearance) => dbContext.PlayerMatchAppearances.Remove(appearance);
    public void RemoveSubstitution(MatchSubstitution substitution) => dbContext.MatchSubstitutions.Remove(substitution);
    public Task SaveChangesAsync(CancellationToken ct) => dbContext.SaveChangesAsync(ct);
}
