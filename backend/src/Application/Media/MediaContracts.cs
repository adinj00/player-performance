using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Media;
using PlayerPerformance.Application.Files;
namespace PlayerPerformance.Application.Media;

public sealed record CreateExternalMediaReferenceRequest(Guid TeamId, MediaCategory Category, string? Title, string? Description, string? Url, string? ProviderLabel);
public sealed record UpdateMediaRequest(MediaCategory Category, string? Title, string? Description, string? Url, string? ProviderLabel);
public sealed record CreateMediaAssetRequest(Guid TeamId, MediaCategory Category, string? Title, string? Description, string? OriginalFileName, string? ContentType, long? DeclaredLength, Stream Content);
public sealed record MediaListQuery(Guid? TeamId = null, MediaSourceType? SourceType = null, MediaCategory? Category = null, string? Search = null, bool IncludeArchived = false, int Page = 1, int PageSize = 25, MediaLinkTargetType? LinkedTargetType = null, Guid? LinkedTargetId = null);
public sealed record MediaSourceResponse(MediaSourceType SourceType, string? OriginalFileName, string? ContentType, long? SizeBytes, string? Url, string? ProviderLabel);
public sealed record MediaResponse(Guid Id, Guid TeamId, string TeamName, MediaSourceType SourceType, MediaCategory Category, string Title, string? Description, MediaSourceResponse Source, Guid CreatedByUserId, DateTime CreatedAtUtc, DateTime UpdatedAtUtc, bool IsArchived, DateTime? ArchivedAtUtc);
public sealed record PagedMediaResponse(IReadOnlyList<MediaResponse> Items, int Page, int PageSize, int TotalCount, int TotalPages);
public enum MediaLinkTargetType
{
    MATCH, MATCH_REPORT, PLAYER
}
public sealed record MediaLinkCandidateResponse(Guid Id, MediaLinkTargetType TargetType, string PrimaryLabel, string SecondaryLabel, string Status);
public sealed record PagedMediaLinkCandidatesResponse(IReadOnlyList<MediaLinkCandidateResponse> Items, int Page, int PageSize, int TotalCount, int TotalPages);
public sealed record MediaContentResponse(Stream Content, string ContentType, string OriginalFileName);
public interface IMediaService
{
    Task<Result<PagedMediaResponse>> ListAsync(MediaListQuery query, CancellationToken ct);
    Task<MediaResponse?> GetAsync(Guid id, CancellationToken ct);
    Task<Result<MediaResponse>> CreateExternalAsync(CreateExternalMediaReferenceRequest request, CancellationToken ct);
    Task<Result<MediaResponse>> CreateAssetAsync(CreateMediaAssetRequest request, CancellationToken ct);
    Task<Result<MediaResponse>> UpdateAsync(Guid id, UpdateMediaRequest request, CancellationToken ct);
    Task<Result<MediaResponse>> ArchiveAsync(Guid id, CancellationToken ct);
    Task<Result<MediaResponse>> RestoreAsync(Guid id, CancellationToken ct);
    Task<Result<MediaContentResponse>> OpenContentAsync(Guid id, CancellationToken ct);
    Task<Result> LinkAsync(Guid mediaId, MediaLinkTargetType targetType, Guid targetId, CancellationToken ct);
    Task<Result> UnlinkAsync(Guid mediaId, MediaLinkTargetType targetType, Guid targetId, CancellationToken ct);
    Task<Result<PagedMediaLinkCandidatesResponse>> ListCandidatesAsync(Guid mediaId, MediaLinkTargetType targetType, string? search, int page, int pageSize, CancellationToken ct);
}

public static class MediaUploadRules
{
    public static readonly IReadOnlyDictionary<MediaCategory, IReadOnlyDictionary<string, string>> Accepted = new Dictionary<MediaCategory, IReadOnlyDictionary<string, string>>
    {
        [MediaCategory.VIDEO] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".mp4"] = "video/mp4",
            [".webm"] = "video/webm",
            [".mov"] = "video/quicktime"
        },
        [MediaCategory.IMAGE] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        },
        [MediaCategory.DOCUMENT] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf"
        }
    };
    public static bool IsAllowed(MediaCategory category, string fileName, string contentType) => Accepted.TryGetValue(category, out var types) && types.TryGetValue(Path.GetExtension(fileName), out var expected) && string.Equals(expected, contentType.Trim(), StringComparison.OrdinalIgnoreCase);
}
