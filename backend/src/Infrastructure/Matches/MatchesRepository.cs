using Microsoft.EntityFrameworkCore;
using PlayerPerformance.Application.Matches;
using PlayerPerformance.Domain.Matches;
using PlayerPerformance.Domain.Teams;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.Infrastructure.Matches;

internal sealed class MatchesRepository(AppDbContext dbContext) : IMatchesRepository
{
    public Task<Match?> GetAsync(Guid id, CancellationToken ct) => dbContext.Matches.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<MatchReadModel?> GetReadAsync(Guid id, IReadOnlyCollection<Guid>? teamScope, CancellationToken ct) => (await ReadQuery(teamScope).SingleOrDefaultAsync(x => x.Match.Id == id, ct)) is { } x ? ToReadModel(x) : null;
    public async Task<PagedMatchReadModel> ListAsync(MatchListQuery query, IReadOnlyCollection<Guid>? teamScope, CancellationToken ct)
    {
        var source = ReadQuery(teamScope).Where(x => query.IncludeArchived || !x.Match.IsArchived);
        if (query.SeasonId is { } seasonId)
            source = source.Where(x => x.Match.SeasonId == seasonId);
        if (query.TeamId is { } teamId)
            source = source.Where(x => x.Match.TeamId == teamId);
        if (query.CompetitionId is { } competitionId)
            source = source.Where(x => x.Match.CompetitionId == competitionId);
        if (query.OpponentId is { } opponentId)
            source = source.Where(x => x.Match.OpponentId == opponentId);
        if (query.Status is { } status)
            source = source.Where(x => x.Match.Status == status);
        if (query.DateFrom is { } dateFrom)
            source = source.Where(x => x.Match.KickoffAtUtc >= dateFrom);
        if (query.DateTo is { } dateTo)
            source = source.Where(x => x.Match.KickoffAtUtc <= dateTo);
        var total = await source.CountAsync(ct);
        var items = await source.OrderByDescending(x => x.Match.KickoffAtUtc).ThenBy(x => x.Match.Id).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new(items.Select(ToReadModel).ToArray(), total);
    }
    public Task<bool> ExactDuplicateExistsAsync(Guid teamId, Guid opponentId, DateTime kickoffAtUtc, CancellationToken ct) => dbContext.Matches.AnyAsync(x => !x.IsArchived && x.TeamId == teamId && x.OpponentId == opponentId && x.KickoffAtUtc == kickoffAtUtc, ct);
    public async Task<MatchReferences?> GetActiveReferencesAsync(Guid seasonId, Guid competitionId, Guid teamId, Guid opponentId, Guid? venueId, CancellationToken ct)
    {
        var season = await dbContext.Seasons.AsNoTracking().SingleOrDefaultAsync(x => x.Id == seasonId && !x.IsArchived, ct);
        if (season is null || !await dbContext.Competitions.AnyAsync(x => x.Id == competitionId && !x.IsArchived, ct) || !await dbContext.Teams.AnyAsync(x => x.Id == teamId && x.Status == TeamStatus.ACTIVE, ct) || !await dbContext.Opponents.AnyAsync(x => x.Id == opponentId && !x.IsArchived, ct) || (venueId.HasValue && !await dbContext.Venues.AnyAsync(x => x.Id == venueId && !x.IsArchived, ct)))
            return null;
        return new(season.StartDate, season.EndDate);
    }
    public void Add(Match match) => dbContext.Matches.Add(match);
    public Task SaveChangesAsync(CancellationToken ct) => dbContext.SaveChangesAsync(ct);
    private IQueryable<MatchProjection> ReadQuery(IReadOnlyCollection<Guid>? teamScope) => from match in dbContext.Matches.AsNoTracking() join season in dbContext.Seasons.AsNoTracking() on match.SeasonId equals season.Id join competition in dbContext.Competitions.AsNoTracking() on match.CompetitionId equals competition.Id join team in dbContext.Teams.AsNoTracking() on match.TeamId equals team.Id join opponent in dbContext.Opponents.AsNoTracking() on match.OpponentId equals opponent.Id join venue in dbContext.Venues.AsNoTracking() on match.VenueId equals venue.Id into venues from venue in venues.DefaultIfEmpty() where teamScope == null || teamScope.Contains(match.TeamId) select new MatchProjection(match, season.Name, competition.Name, team.Name, opponent.Name, venue == null ? null : venue.Name);
    private static MatchReadModel ToReadModel(MatchProjection x) => new(x.Match, x.SeasonName, x.CompetitionName, x.TeamName, x.OpponentName, x.VenueName);
    private sealed record MatchProjection(Match Match, string SeasonName, string CompetitionName, string TeamName, string OpponentName, string? VenueName);
}
