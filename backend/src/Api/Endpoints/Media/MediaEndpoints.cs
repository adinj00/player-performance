using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using PlayerPerformance.Api.Authentication;
using PlayerPerformance.Application.Media;
using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Media;
using PlayerPerformance.Application.Auditing;
using PlayerPerformance.Domain.Auditing;
namespace PlayerPerformance.Api.Endpoints.Media;

internal static class MediaEndpoints
{
    private static readonly JsonSerializerOptions MultipartJsonOptions = CreateMultipartJsonOptions();

    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var media = endpoints.MapGroup("/api/media").RequireAuthorization().WithTags("Media");
        media.MapGet("", ListAsync);
        media.MapGet("/capabilities", GetCapabilities);
        media.MapGet("/{mediaId:guid}", GetAsync);
        media.MapGet("/{mediaId:guid}/content", ContentAsync);
        media.MapGet("/{mediaId:guid}/audit", AuditAsync);
        media.MapGet("/{mediaId:guid}/link-candidates", CandidatesAsync);
        media.MapPost("/assets", CreateAssetAsync);
        media.MapPost("/external-references", CreateExternalAsync);
        media.MapPatch("/{mediaId:guid}", UpdateAsync);
        media.MapPost("/{mediaId:guid}/archive", ArchiveAsync);
        media.MapPost("/{mediaId:guid}/restore", RestoreAsync);
        media.MapPost("/{mediaId:guid}/{targetType}/{targetId:guid}", LinkAsync);
        media.MapDelete("/{mediaId:guid}/{targetType}/{targetId:guid}", UnlinkAsync);
        return endpoints;
    }
    private static async Task<IResult> ListAsync(IMediaService s, HttpContext h, CancellationToken ct, Guid? teamId = null, MediaSourceType? sourceType = null, MediaCategory? category = null, string? search = null, bool includeArchived = false, int page = 1, int pageSize = 25, MediaLinkTargetType? linkedTargetType = null, Guid? linkedTargetId = null) => ToResult(await s.ListAsync(new(teamId, sourceType, category, search, includeArchived, page, pageSize, linkedTargetType, linkedTargetId), ct), h);
    private static IResult GetCapabilities(Microsoft.Extensions.Options.IOptions<PlayerPerformance.Infrastructure.Media.MediaOptions> options) => TypedResults.Ok(new
    {
        maxUploadSizeBytes = options.Value.MaxUploadSizeBytes,
        uploadedFileTypes = MediaUploadRules.Accepted.OrderBy(x => x.Key).Select(x => new
        {
            category = x.Key,
            extensions = x.Value.Keys.OrderBy(x => x),
            contentTypes = x.Value.Values.Distinct().OrderBy(x => x)
        }),
        externalReferenceCategories = Enum.GetValues<MediaCategory>()
    });
    private static async Task<IResult> GetAsync(Guid mediaId, IMediaService s, CancellationToken ct) => (await s.GetAsync(mediaId, ct)) is { } r ? TypedResults.Ok(r) : TypedResults.NotFound();
    private static async Task<IResult> ContentAsync(Guid mediaId, IMediaService s, HttpContext h, CancellationToken ct, bool download = false)
    {
        var result = await s.OpenContentAsync(mediaId, ct);
        if (result.IsFailure)
            return Problem(result.Error, h);
        h.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        return Results.File(result.Value.Content, result.Value.ContentType, download ? result.Value.OriginalFileName : null, enableRangeProcessing: result.Value.Content.CanSeek, lastModified: null, entityTag: null);
    }
    private static async Task<IResult> AuditAsync(Guid mediaId, IMediaService s, IAuditHistoryRepository audits, int page = 1, int pageSize = 25, string? action = null, DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default)
    {
        if (await s.GetAsync(mediaId, ct) is null)
            return TypedResults.NotFound();
        var q = new AuditHistoryQuery(page, pageSize, action, dateFrom, dateTo);
        return !AuditHistoryValidation.IsValid(q) ? TypedResults.BadRequest() : TypedResults.Ok(await audits.ListAsync(AuditEntityTypes.MediaItem, mediaId, q, ct));
    }
    private static async Task<IResult> CandidatesAsync(Guid mediaId, MediaLinkTargetType targetType, IMediaService s, HttpContext h, string? search = null, int page = 1, int pageSize = 25, CancellationToken ct = default) => ToResult(await s.ListCandidatesAsync(mediaId, targetType, search, page, pageSize, ct), h);
    private static async Task<IResult> CreateExternalAsync(HttpContext h, IAntiforgery a, CreateExternalMediaReferenceRequest r, IMediaService s, CancellationToken ct)
    {
        var fail = await AntiforgeryValidation.ValidateRequestAsync(h, a);
        return fail ?? ToCreated(await s.CreateExternalAsync(r, ct), h);
    }
    private static async Task<IResult> CreateAssetAsync(HttpContext h, IAntiforgery a, IMediaService s, CancellationToken ct)
    {
        var csrf = await AntiforgeryValidation.ValidateRequestAsync(h, a);
        if (csrf is not null)
            return csrf;
        if (!MediaTypeHeaderValue.TryParse(h.Request.ContentType, out var mediaType) || string.IsNullOrEmpty(mediaType.Boundary.Value))
            return TypedResults.BadRequest();
        var reader = new MultipartReader(mediaType.Boundary.Value, h.Request.Body);
        var metadata = await reader.ReadNextSectionAsync(ct);
        if (metadata?.ContentDisposition is null || !ContentDispositionHeaderValue.TryParse(metadata.ContentDisposition, out var metadataDisposition) || !string.Equals(HeaderUtilities.RemoveQuotes(metadataDisposition!.Name).Value, "metadata", StringComparison.Ordinal))
            return TypedResults.BadRequest();
        if (metadata.Body.CanSeek && metadata.Body.Length > 64 * 1024)
            return TypedResults.BadRequest();
        MediaAssetMetadata? request;
        try
        {
            request = await JsonSerializer.DeserializeAsync<MediaAssetMetadata>(metadata.Body, MultipartJsonOptions, ct);
        }
        catch (JsonException)
        {
            return TypedResults.BadRequest();
        }
        var file = await reader.ReadNextSectionAsync(ct);
        if (request is null || file?.ContentDisposition is null || !ContentDispositionHeaderValue.TryParse(file.ContentDisposition, out var fileDisposition) || string.IsNullOrWhiteSpace(fileDisposition!.FileName.Value))
            return TypedResults.BadRequest();
        var name = HeaderUtilities.RemoveQuotes(fileDisposition.FileName).Value;
        return ToCreated(await s.CreateAssetAsync(new(request.TeamId, request.Category, request.Title, request.Description, name, file.ContentType, h.Request.ContentLength, file.Body), ct), h);
    }
    private static async Task<IResult> UpdateAsync(Guid mediaId, HttpContext h, IAntiforgery a, UpdateMediaRequest r, IMediaService s, CancellationToken ct)
    {
        var fail = await AntiforgeryValidation.ValidateRequestAsync(h, a);
        return fail ?? ToResult(await s.UpdateAsync(mediaId, r, ct), h);
    }
    private static async Task<IResult> ArchiveAsync(Guid mediaId, HttpContext h, IAntiforgery a, IMediaService s, CancellationToken ct)
    {
        var fail = await AntiforgeryValidation.ValidateRequestAsync(h, a);
        return fail ?? ToResult(await s.ArchiveAsync(mediaId, ct), h);
    }
    private static async Task<IResult> RestoreAsync(Guid mediaId, HttpContext h, IAntiforgery a, IMediaService s, CancellationToken ct)
    {
        var fail = await AntiforgeryValidation.ValidateRequestAsync(h, a);
        return fail ?? ToResult(await s.RestoreAsync(mediaId, ct), h);
    }
    private static async Task<IResult> LinkAsync(Guid mediaId, MediaLinkTargetType targetType, Guid targetId, HttpContext h, IAntiforgery a, IMediaService s, CancellationToken ct)
    {
        var fail = await AntiforgeryValidation.ValidateRequestAsync(h, a);
        return fail ?? ToResult(await s.LinkAsync(mediaId, targetType, targetId, ct), h);
    }
    private static async Task<IResult> UnlinkAsync(Guid mediaId, MediaLinkTargetType targetType, Guid targetId, HttpContext h, IAntiforgery a, IMediaService s, CancellationToken ct)
    {
        var fail = await AntiforgeryValidation.ValidateRequestAsync(h, a);
        return fail ?? ToResult(await s.UnlinkAsync(mediaId, targetType, targetId, ct), h);
    }
    private static IResult ToCreated(Result<MediaResponse> r, HttpContext h) => r.IsSuccess ? TypedResults.Created($"/api/media/{r.Value.Id}", r.Value) : Problem(r.Error, h);
    private static JsonSerializerOptions CreateMultipartJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
    private static IResult ToResult(Result r, HttpContext h) => r.IsSuccess ? TypedResults.NoContent() : Problem(r.Error, h);
    private static IResult ToResult<T>(Result<T> r, HttpContext h) => r.IsSuccess ? TypedResults.Ok(r.Value) : Problem(r.Error, h);
    private static IResult Problem(Error e, HttpContext h) => TypedResults.Problem(new ProblemDetails
    {
        Status = e.Code switch
        {
            "not_found" => 404,
            "forbidden" => 403,
            "media_conflict" => 409,
            "validation_failed" => 422,
            _ => 400
        },
        Title = "Request could not be completed",
        Detail = e.Message,
        Extensions = { { "code", e.Code }, { "traceId", h.TraceIdentifier } }
    });
}

internal sealed record MediaAssetMetadata(Guid TeamId, MediaCategory Category, string? Title, string? Description);
