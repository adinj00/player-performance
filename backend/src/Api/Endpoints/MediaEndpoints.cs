using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using PlayerPerformance.Api.Authentication;
using PlayerPerformance.Application.Media;
using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Media;
namespace PlayerPerformance.Api.Endpoints.Media;

internal static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var media = endpoints.MapGroup("/api/media").RequireAuthorization().WithTags("Media");
        media.MapGet("", ListAsync);
        media.MapGet("/{mediaId:guid}", GetAsync);
        media.MapPost("/external-references", CreateExternalAsync);
        media.MapPatch("/{mediaId:guid}", UpdateAsync);
        media.MapPost("/{mediaId:guid}/archive", ArchiveAsync);
        media.MapPost("/{mediaId:guid}/restore", RestoreAsync);
        return endpoints;
    }
    private static async Task<IResult> ListAsync(IMediaService s, HttpContext h, CancellationToken ct, Guid? teamId = null, MediaSourceType? sourceType = null, MediaCategory? category = null, string? search = null, bool includeArchived = false, int page = 1, int pageSize = 25) => ToResult(await s.ListAsync(new(teamId, sourceType, category, search, includeArchived, page, pageSize), ct), h);
    private static async Task<IResult> GetAsync(Guid mediaId, IMediaService s, CancellationToken ct) => (await s.GetAsync(mediaId, ct)) is { } r ? TypedResults.Ok(r) : TypedResults.NotFound();
    private static async Task<IResult> CreateExternalAsync(HttpContext h, IAntiforgery a, CreateExternalMediaReferenceRequest r, IMediaService s, CancellationToken ct)
    {
        var fail = await AntiforgeryValidation.ValidateRequestAsync(h, a);
        return fail ?? ToCreated(await s.CreateExternalAsync(r, ct), h);
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
    private static IResult ToCreated(Result<MediaResponse> r, HttpContext h) => r.IsSuccess ? TypedResults.Created($"/api/media/{r.Value.Id}", r.Value) : Problem(r.Error, h); private static IResult ToResult<T>(Result<T> r, HttpContext h) => r.IsSuccess ? TypedResults.Ok(r.Value) : Problem(r.Error, h); private static IResult Problem(Error e, HttpContext h) => TypedResults.Problem(new ProblemDetails
    {
        Status = e.Code switch { "not_found" => 404, "forbidden" => 403, "media_conflict" => 409, "validation_failed" => 422, _ => 400 },
        Title = "Request could not be completed",
        Detail = e.Message,
        Extensions = { { "code", e.Code }, { "traceId", h.TraceIdentifier } }
    });
}
