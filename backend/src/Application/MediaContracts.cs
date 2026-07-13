using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Media;
namespace PlayerPerformance.Application.Media;

public sealed record CreateExternalMediaReferenceRequest(Guid TeamId, MediaCategory Category, string? Title, string? Description, string? Url, string? ProviderLabel);
public sealed record UpdateMediaRequest(MediaCategory Category, string? Title, string? Description, string? Url, string? ProviderLabel);
public sealed record MediaListQuery(Guid? TeamId = null, MediaSourceType? SourceType = null, MediaCategory? Category = null, string? Search = null, bool IncludeArchived = false, int Page = 1, int PageSize = 25);
public sealed record MediaSourceResponse(MediaSourceType SourceType, string? OriginalFileName, string? ContentType, long? SizeBytes, string? Url, string? ProviderLabel);
public sealed record MediaResponse(Guid Id, Guid TeamId, string TeamName, MediaSourceType SourceType, MediaCategory Category, string Title, string? Description, MediaSourceResponse Source, Guid CreatedByUserId, DateTime CreatedAtUtc, DateTime UpdatedAtUtc, bool IsArchived, DateTime? ArchivedAtUtc);
public sealed record PagedMediaResponse(IReadOnlyList<MediaResponse> Items, int Page, int PageSize, int TotalCount, int TotalPages);
public interface IMediaService
{
    Task<Result<PagedMediaResponse>> ListAsync(MediaListQuery query, CancellationToken ct);
    Task<MediaResponse?> GetAsync(Guid id, CancellationToken ct);
    Task<Result<MediaResponse>> CreateExternalAsync(CreateExternalMediaReferenceRequest request, CancellationToken ct);
    Task<Result<MediaResponse>> UpdateAsync(Guid id, UpdateMediaRequest request, CancellationToken ct);
    Task<Result<MediaResponse>> ArchiveAsync(Guid id, CancellationToken ct);
    Task<Result<MediaResponse>> RestoreAsync(Guid id, CancellationToken ct);
}
