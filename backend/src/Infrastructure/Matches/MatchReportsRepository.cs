using Microsoft.EntityFrameworkCore;
using PlayerPerformance.Application.Matches;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.Infrastructure.Matches;

internal sealed class MatchReportsRepository(AppDbContext dbContext) : IMatchReportsRepository
{
    public async Task<MatchReportAggregate?> GetAggregateAsync(Guid reportId, CancellationToken ct) => await AggregateQuery().SingleOrDefaultAsync(x => x.Report.Id == reportId, ct);
    public async Task<MatchReportAggregate?> GetByMatchAggregateAsync(Guid matchId, CancellationToken ct) => await AggregateQuery().SingleOrDefaultAsync(x => x.Report.MatchId == matchId, ct);
    public async Task<MatchReportReadModel?> GetReadByMatchAsync(Guid matchId, IReadOnlyCollection<Guid>? scope, CancellationToken ct) => await ReadQuery(scope).SingleOrDefaultAsync(x => x.Report.MatchId == matchId, ct);
    public async Task<PagedMatchReportReadModel> ListAsync(MatchReportListQuery query, IReadOnlyCollection<Guid>? scope, IReadOnlyCollection<MatchReportStatus> visibleStatuses, CancellationToken ct)
    {
        var source = ReadQuery(scope).Where(x => visibleStatuses.Contains(x.Report.Status));
        if (query.SeasonId.HasValue)
            source = source.Where(x => x.Match.SeasonId == query.SeasonId.Value);
        if (query.TeamId.HasValue)
            source = source.Where(x => x.Match.TeamId == query.TeamId.Value);
        if (query.CompetitionId.HasValue)
            source = source.Where(x => x.Match.CompetitionId == query.CompetitionId.Value);
        if (query.Status.HasValue)
            source = source.Where(x => x.Report.Status == query.Status.Value);
        if (query.DateFrom.HasValue)
            source = source.Where(x => x.Match.KickoffAtUtc >= query.DateFrom.Value);
        if (query.DateTo.HasValue)
            source = source.Where(x => x.Match.KickoffAtUtc <= query.DateTo.Value);
        var total = await source.CountAsync(ct);
        var items = await source.OrderByDescending(x => x.Match.KickoffAtUtc).ThenBy(x => x.Report.Id).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new(items, total);
    }
    public Task<bool> ExistsForMatchAsync(Guid matchId, CancellationToken ct) => dbContext.MatchReports.AnyAsync(x => x.MatchId == matchId, ct);
    public void Add(MatchReport report) => dbContext.MatchReports.Add(report);
    public Task SaveChangesAsync(CancellationToken ct) => dbContext.SaveChangesAsync(ct);
    private IQueryable<MatchReportAggregate> AggregateQuery() => from report in dbContext.MatchReports join match in dbContext.Matches on report.MatchId equals match.Id select new MatchReportAggregate(report, match, dbContext.PlayerMatchAppearances.Count(x => x.MatchId == match.Id));
    private IQueryable<MatchReportReadModel> ReadQuery(IReadOnlyCollection<Guid>? scope) => from report in dbContext.MatchReports.AsNoTracking() join match in dbContext.Matches.AsNoTracking() on report.MatchId equals match.Id join team in dbContext.Teams.AsNoTracking() on match.TeamId equals team.Id join opponent in dbContext.Opponents.AsNoTracking() on match.OpponentId equals opponent.Id join competition in dbContext.Competitions.AsNoTracking() on match.CompetitionId equals competition.Id where scope == null || scope.Contains(match.TeamId) select new MatchReportReadModel(report, match, team.Name, opponent.Name, competition.Name);
}
