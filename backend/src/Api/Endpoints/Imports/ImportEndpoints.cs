using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using PlayerPerformance.Api.Authentication;
using PlayerPerformance.Application.Auditing;
using PlayerPerformance.Application.Imports;
using PlayerPerformance.Domain.Auditing;
using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;
using PlayerPerformance.Domain.Imports;

namespace PlayerPerformance.Api.Endpoints.Imports;

internal static class ImportEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    public static IEndpointRouteBuilder MapImportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/imports").RequireAuthorization().WithTags("Imports");
        group.MapGet("/capabilities", CapabilitiesAsync);
        group.MapPost("", CreateAsync);
        group.MapGet("", ListAsync);
        group.MapGet("/{id:guid}", GetAsync);
        group.MapGet("/{id:guid}/source", SourceAsync);
        group.MapGet("/{id:guid}/preview", PreviewAsync);
        group.MapGet("/{id:guid}/validation-issues", IssuesAsync);
        group.MapGet("/{id:guid}/audit", AuditAsync);
        group.MapPost("/{id:guid}/preview", (Guid id, HttpContext h, IAntiforgery a, IImportService s, CancellationToken ct) => ActionAsync(id, ImportProcessingOperation.PREVIEW, h, a, s, ct));
        group.MapPost("/{id:guid}/validate", (Guid id, HttpContext h, IAntiforgery a, IImportService s, CancellationToken ct) => ActionAsync(id, ImportProcessingOperation.VALIDATION, h, a, s, ct));
        group.MapPost("/{id:guid}/confirm", (Guid id, HttpContext h, IAntiforgery a, IImportService s, CancellationToken ct) => ActionAsync(id, ImportProcessingOperation.CONFIRMATION, h, a, s, ct));
        group.MapPost("/{id:guid}/cancel", CancelAsync);
        return endpoints;
    }
    private static async Task<IResult> CapabilitiesAsync(IImportService s, HttpContext h, CancellationToken ct) => ToResult(await s.GetCapabilitiesAsync(ct), h);
    private static async Task<IResult> CreateAsync(HttpContext h, IAntiforgery anti, IImportService s, CancellationToken ct)
    {
        var csrf = await AntiforgeryValidation.ValidateRequestAsync(h, anti);
        if (csrf is not null)
            return csrf;
        if (!MediaTypeHeaderValue.TryParse(h.Request.ContentType, out var type) || string.IsNullOrEmpty(type.Boundary.Value))
            return TypedResults.BadRequest();
        var reader = new MultipartReader(type.Boundary.Value, h.Request.Body);
        var metadata = await reader.ReadNextSectionAsync(ct);
        if (!IsPart(metadata, "metadata", false))
            return TypedResults.BadRequest();
        ImportMetadata? request;
        try
        {
            request = await JsonSerializer.DeserializeAsync<ImportMetadata>(metadata!.Body, JsonOptions, ct);
        }
        catch (JsonException)
        {
            return TypedResults.BadRequest();
        }
        var file = await reader.ReadNextSectionAsync(ct);
        if (request is null || !IsPart(file, "file", true))
            return TypedResults.BadRequest();
        var name = HeaderUtilities.RemoveQuotes(ContentDispositionHeaderValue.Parse(file!.ContentDisposition!).FileName).Value;
        return ToCreated(await s.CreateAsync(new(request.TeamId, request.MatchId, request.TrainingSessionId, request.ImportType, request.SourceSystem, request.SourceLabel, request.Description, name!, file.ContentType, h.Request.ContentLength, file.Body), ct), h);
    }
    private static async Task<IResult> ListAsync(IImportService s, HttpContext h, CancellationToken ct, Guid? teamId = null, Guid? matchId = null, ImportType? importType = null, ImportSourceSystem? sourceSystem = null, ImportFileFormat? fileFormat = null, ImportJobStatus? status = null, string? search = null, DateTime? dateFrom = null, DateTime? dateTo = null, int page = 1, int pageSize = 25) => ToResult(await s.ListAsync(new(teamId, matchId, importType, sourceSystem, fileFormat, status, search, dateFrom, dateTo, page, pageSize), ct), h);
    private static async Task<IResult> GetAsync(Guid id, IImportService s, CancellationToken ct) => await s.GetAsync(id, ct) is { } item ? TypedResults.Ok(item) : TypedResults.NotFound();
    private static async Task<IResult> SourceAsync(Guid id, IImportService s, HttpContext h, CancellationToken ct)
    {
        var result = await s.OpenSourceAsync(id, ct);
        if (result.IsFailure)
            return Problem(result.Error, h);
        h.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        return Results.File(result.Value.Content, result.Value.ContentType, result.Value.OriginalFileName, enableRangeProcessing: result.Value.Content.CanSeek);
    }
    private static async Task<IResult> PreviewAsync(Guid id, IImportService s, HttpContext h, int page = 1, int pageSize = 25, CancellationToken ct = default) => ToResult(await s.GetPreviewAsync(id, page, pageSize, ct), h);
    private static async Task<IResult> IssuesAsync(Guid id, IImportService s, HttpContext h, ImportValidationSeverity? severity = null, int page = 1, int pageSize = 25, CancellationToken ct = default) => ToResult(await s.GetIssuesAsync(id, severity, page, pageSize, ct), h);
    private static async Task<IResult> AuditAsync(Guid id, IImportService s, IAuditHistoryRepository audits, int page = 1, int pageSize = 25, string? action = null, DateTime? dateFrom = null, DateTime? dateTo = null, CancellationToken ct = default)
    {
        if (await s.GetAsync(id, ct) is null)
            return TypedResults.NotFound();
        var query = new AuditHistoryQuery(page, pageSize, action, dateFrom, dateTo);
        return !AuditHistoryValidation.IsValid(query) ? TypedResults.BadRequest() : TypedResults.Ok(await audits.ListAsync(AuditEntityTypes.ImportJob, id, query, ct));
    }
    private static async Task<IResult> ActionAsync(Guid id, ImportProcessingOperation operation, HttpContext h, IAntiforgery anti, IImportService s, CancellationToken ct)
    {
        var csrf = await AntiforgeryValidation.ValidateRequestAsync(h, anti);
        return csrf ?? ToResult(await s.ExecuteAsync(id, operation, ct), h);
    }
    private static async Task<IResult> CancelAsync(Guid id, HttpContext h, IAntiforgery anti, IImportService s, CancellationToken ct)
    {
        var csrf = await AntiforgeryValidation.ValidateRequestAsync(h, anti);
        return csrf ?? ToResult(await s.CancelAsync(id, ct), h);
    }
    private static bool IsPart(MultipartSection? section, string name, bool file)
    {
        if (section?.ContentDisposition is null || !ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var disposition) || !string.Equals(HeaderUtilities.RemoveQuotes(disposition!.Name).Value, name, StringComparison.Ordinal))
            return false;
        return !file || !string.IsNullOrWhiteSpace(disposition.FileName.Value);
    }
    private static IResult ToCreated(Result<ImportJobResponse> r, HttpContext h) => r.IsSuccess ? TypedResults.Created($"/api/imports/{r.Value.Id}", r.Value) : Problem(r.Error, h);
    private static IResult ToResult(Result r, HttpContext h) => r.IsSuccess ? TypedResults.NoContent() : Problem(r.Error, h);
    private static IResult ToResult<T>(Result<T> r, HttpContext h) => r.IsSuccess ? TypedResults.Ok(r.Value) : Problem(r.Error, h);
    private static IResult Problem(Error error, HttpContext h) => TypedResults.Problem(new ProblemDetails
    {
        Status = error.Code switch
        {
            "not_found" => 404,
            "forbidden" => 403,
            "unsupported_processing" or "import_conflict" => 409,
            "validation_failed" => 422,
            "file_storage.object_too_large" => 413,
            "file_storage.storage_unavailable" => 500,
            _ => 400
        },
        Title = "Request could not be completed",
        Detail = error.Message,
        Extensions = { ["code"] = error.Code, ["traceId"] = h.TraceIdentifier
        }
    });
    private sealed record ImportMetadata(Guid TeamId, Guid? MatchId, Guid? TrainingSessionId, ImportType ImportType, ImportSourceSystem SourceSystem, string? SourceLabel, string? Description);
}
