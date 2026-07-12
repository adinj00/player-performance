using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PlayerPerformance.Application.Auditing;
using PlayerPerformance.Domain.Auditing;
using PlayerPerformance.Infrastructure.Persistence;
namespace PlayerPerformance.Infrastructure.Auditing;
internal sealed class AuditStore(AppDbContext db) : IAuditWriter, IAuditHistoryRepository
{
    public void Add(AuditWrite entry) => db.AuditLogs.Add(AuditLog.Create(Guid.NewGuid(), entry.ActorUserId, entry.Action, entry.EntityType, entry.EntityId, entry.OccurredAtUtc, entry.PreviousValuesJson, entry.NewValuesJson, entry.MetadataJson));
    public async Task<PagedAuditHistoryResponse> ListAsync(string entityType, Guid entityId, AuditHistoryQuery query, CancellationToken ct)
    {
        var source = db.AuditLogs.AsNoTracking().Where(x => x.EntityType == entityType && x.EntityId == entityId);
        if (query.Action is not null)
            source = source.Where(x => x.Action == query.Action);
        if (query.DateFrom.HasValue)
            source = source.Where(x => x.OccurredAtUtc >= query.DateFrom.Value);
        if (query.DateTo.HasValue)
            source = source.Where(x => x.OccurredAtUtc <= query.DateTo.Value);
        var total = await source.CountAsync(ct);
        var rows = await (from log in source
                          join profile in db.StaffAccessProfiles.AsNoTracking() on log.ActorUserId equals profile.UserId into profiles
                          from profile in profiles.DefaultIfEmpty()
                          orderby log.OccurredAtUtc descending, log.Id descending
                          select new
                          {
                              log,
                              DisplayName = profile == null ? "Unknown staff user" : profile.DisplayName
                          }).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new(rows.Select(x => new AuditEntryResponse(x.log.Id, new(x.log.ActorUserId, x.DisplayName), x.log.Action, x.log.EntityType, x.log.EntityId, x.log.OccurredAtUtc, Parse(x.log.PreviousValuesJson), Parse(x.log.NewValuesJson), Parse(x.log.MetadataJson))).ToList(), query.Page, query.PageSize, total, (int)Math.Ceiling(total / (double)query.PageSize));
    }
    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement.Clone();
}
