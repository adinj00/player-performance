using Microsoft.EntityFrameworkCore;
using PlayerPerformance.Application.Players;
using PlayerPerformance.Domain.Players;
using PlayerPerformance.Infrastructure.Persistence;

namespace PlayerPerformance.Infrastructure.Players;

internal sealed class PlayersRepository(AppDbContext dbContext) : IPlayersRepository
{
    public async Task<PagedPlayers> ListAsync(PlayerListQuery query, CancellationToken ct)
    {
        IQueryable<Player> players = dbContext.Players.AsNoTracking();
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
        var items = await players.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ThenBy(x => x.Id).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedPlayers(items, totalCount);
    }

    public Task<Player?> GetAsync(Guid id, CancellationToken ct) => dbContext.Players.SingleOrDefaultAsync(x => x.Id == id, ct);
    public void Add(Player player) => dbContext.Players.Add(player);
    public Task SaveChangesAsync(CancellationToken ct) => dbContext.SaveChangesAsync(ct);
}
