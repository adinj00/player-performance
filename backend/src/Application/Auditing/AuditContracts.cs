using System.Text.Json;
using PlayerPerformance.Domain.Auditing;
namespace PlayerPerformance.Application.Auditing;
public sealed record AuditWrite(Guid ActorUserId, string Action, string EntityType, Guid EntityId, DateTime OccurredAtUtc, string PreviousValuesJson, string NewValuesJson, string MetadataJson);
public sealed record AuditActorResponse(Guid Id, string DisplayName);
public sealed record AuditEntryResponse(Guid Id, AuditActorResponse Actor, string Action, string EntityType, Guid EntityId, DateTime OccurredAtUtc, JsonElement PreviousValues, JsonElement NewValues, JsonElement Metadata);
public sealed record AuditHistoryQuery(int Page = 1, int PageSize = 25, string? Action = null, DateTime? DateFrom = null, DateTime? DateTo = null);
public sealed record PagedAuditHistoryResponse(IReadOnlyList<AuditEntryResponse> Items, int Page, int PageSize, int TotalCount, int TotalPages);
public interface IAuditWriter
{
    void Add(AuditWrite entry);
}
public interface IAuditHistoryRepository
{
    Task<PagedAuditHistoryResponse> ListAsync(string entityType, Guid entityId, AuditHistoryQuery query, CancellationToken ct);
}
public static class AuditPayload
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
    public static string Object(object? value) => JsonSerializer.Serialize(value ?? new { }, Options);
    public static AuditWrite Create(Guid actorUserId, string action, string entityType, Guid entityId, DateTime occurredAtUtc, object? previous = null, object? next = null, object? metadata = null) => new(actorUserId, action, entityType, entityId, occurredAtUtc, Object(previous), Object(next), Object(metadata));
}
public static class AuditHistoryValidation
{
    public static bool IsValid(AuditHistoryQuery query) => query.Page is >= 1 and <= 100000 && query.PageSize is >= 1 and <= 100 && (query.Action is null || AuditActions.IsKnown(query.Action)) && (!query.DateFrom.HasValue || !query.DateTo.HasValue || query.DateFrom <= query.DateTo);
}
