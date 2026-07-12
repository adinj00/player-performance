using Microsoft.EntityFrameworkCore;
using PlayerPerformance.Application.Matches;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.Infrastructure.Matches;

internal sealed class MatchStatisticsRepository(AppDbContext dbContext) : IMatchStatisticsRepository, IMatchStatisticsCleanup
{
    public async Task<MatchStatisticsAggregate?> GetAggregateAsync(Guid reportId, CancellationToken ct)
    {
        var report = await dbContext.MatchReports.SingleOrDefaultAsync(x => x.Id == reportId, ct);
        if (report is null)
            return null;
        var match = await dbContext.Matches.SingleAsync(x => x.Id == report.MatchId, ct);
        var level = await dbContext.Teams.Where(x => x.Id == match.TeamId).Select(x => x.TrackingLevel).SingleAsync(ct);
        var appearances = await dbContext.PlayerMatchAppearances.Where(x => x.MatchId == match.Id).ToListAsync(ct);
        var player = await dbContext.PlayerMatchStats.Where(x => x.MatchReportId == reportId).ToListAsync(ct);
        var goalkeeper = await dbContext.GoalkeeperMatchStats.Where(x => x.MatchReportId == reportId).ToListAsync(ct);
        return new(report, match, level, appearances, player, goalkeeper);
    }
    public async Task<MatchStatisticsReadModel?> GetReadAsync(Guid reportId, IReadOnlyCollection<Guid>? scope, CancellationToken ct)
    {
        var source = from report in dbContext.MatchReports.AsNoTracking() join match in dbContext.Matches.AsNoTracking() on report.MatchId equals match.Id join team in dbContext.Teams.AsNoTracking() on match.TeamId equals team.Id where report.Id == reportId && (scope == null || scope.Contains(match.TeamId)) select new { report, match, team.TrackingLevel };
        var root = await source.SingleOrDefaultAsync(ct);
        if (root is null)
            return null;
        var appearances = await (from appearance in dbContext.PlayerMatchAppearances.AsNoTracking() join player in dbContext.Players.AsNoTracking() on appearance.PlayerId equals player.Id where appearance.MatchId == root.match.Id orderby player.LastName, player.FirstName, player.Id select new MatchStatisticsAppearanceReadModel(appearance, player.FirstName, player.LastName, player.PreferredName)).ToListAsync(ct);
        var playerStats = await dbContext.PlayerMatchStats.AsNoTracking().Where(x => x.MatchReportId == reportId).ToListAsync(ct);
        var goalkeeperStats = await dbContext.GoalkeeperMatchStats.AsNoTracking().Where(x => x.MatchReportId == reportId).ToListAsync(ct);
        return new(root.report, root.match, root.TrackingLevel, appearances, playerStats, goalkeeperStats);
    }
    public void Add(PlayerMatchStats stats) => dbContext.PlayerMatchStats.Add(stats);
    public void Add(GoalkeeperMatchStats stats) => dbContext.GoalkeeperMatchStats.Add(stats);
    public void Remove(GoalkeeperMatchStats stats) => dbContext.GoalkeeperMatchStats.Remove(stats);
    public async Task RemoveForAppearancesAsync(IReadOnlyCollection<Guid> appearanceIds, CancellationToken ct)
    {
        if (appearanceIds.Count == 0)
            return;
        dbContext.PlayerMatchStats.RemoveRange(await dbContext.PlayerMatchStats.Where(x => appearanceIds.Contains(x.PlayerMatchAppearanceId)).ToListAsync(ct));
        dbContext.GoalkeeperMatchStats.RemoveRange(await dbContext.GoalkeeperMatchStats.Where(x => appearanceIds.Contains(x.PlayerMatchAppearanceId)).ToListAsync(ct));
    }
    public Task SaveChangesAsync(CancellationToken ct) => dbContext.SaveChangesAsync(ct);
}
