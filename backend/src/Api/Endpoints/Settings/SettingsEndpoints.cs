using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using PlayerPerformance.Api.Authentication;
using PlayerPerformance.Api.Authorization;
using PlayerPerformance.Application.Settings;
using PlayerPerformance.Domain.Common.Errors;
using PlayerPerformance.Domain.Common.Results;

namespace PlayerPerformance.Api.Endpoints.Settings;

internal static class SettingsEndpoints
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var settings = endpoints.MapGroup("/api/settings").RequireAuthorization(StaffAuthorizationPolicies.AdminOnly).WithTags("Settings");
        var seasons = settings.MapGroup("/seasons");
        seasons.MapGet("", (bool includeArchived, ISettingsService service, CancellationToken ct) => service.ListSeasonsAsync(includeArchived, ct));
        seasons.MapGet("/{id:guid}", GetSeasonAsync);
        seasons.MapPost("/", CreateSeasonAsync);
        seasons.MapPatch("/{id:guid}", UpdateSeasonAsync);
        seasons.MapPost("/{id:guid}/archive", ArchiveSeasonAsync);
        seasons.MapPost("/{id:guid}/restore", RestoreSeasonAsync);
        var competitions = settings.MapGroup("/competitions");
        competitions.MapGet("", (bool includeArchived, ISettingsService service, CancellationToken ct) => service.ListCompetitionsAsync(includeArchived, ct));
        competitions.MapGet("/{id:guid}", GetCompetitionAsync);
        competitions.MapPost("/", CreateCompetitionAsync);
        competitions.MapPatch("/{id:guid}", UpdateCompetitionAsync);
        competitions.MapPost("/{id:guid}/archive", ArchiveCompetitionAsync);
        competitions.MapPost("/{id:guid}/restore", RestoreCompetitionAsync);
        return endpoints;
    }

    private static async Task<IResult> GetSeasonAsync(Guid id, ISettingsService service, CancellationToken ct) => (await service.GetSeasonAsync(id, ct)) is { } response ? TypedResults.Ok(response) : TypedResults.NotFound();
    private static async Task<IResult> GetCompetitionAsync(Guid id, ISettingsService service, CancellationToken ct) => (await service.GetCompetitionAsync(id, ct)) is { } response ? TypedResults.Ok(response) : TypedResults.NotFound();
    private static async Task<IResult> CreateSeasonAsync(HttpContext context, IAntiforgery antiforgery, CreateSeasonRequest request, ISettingsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        if (failure is not null)
            return failure;
        var result = await service.CreateSeasonAsync(request, ct);
        return result.IsSuccess ? TypedResults.Created($"/api/settings/seasons/{result.Value.Id}", result.Value) : ToProblem(result.Error, context);
    }
    private static async Task<IResult> UpdateSeasonAsync(Guid id, HttpContext context, IAntiforgery antiforgery, UpdateSeasonRequest request, ISettingsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        if (failure is not null)
            return failure;
        return ToResult(await service.UpdateSeasonAsync(id, request, ct), context);
    }
    private static async Task<IResult> ArchiveSeasonAsync(Guid id, HttpContext context, IAntiforgery antiforgery, ISettingsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        if (failure is not null)
            return failure;
        return ToResult(await service.ArchiveSeasonAsync(id, ct), context);
    }
    private static async Task<IResult> RestoreSeasonAsync(Guid id, HttpContext context, IAntiforgery antiforgery, ISettingsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        if (failure is not null)
            return failure;
        return ToResult(await service.RestoreSeasonAsync(id, ct), context);
    }
    private static async Task<IResult> CreateCompetitionAsync(HttpContext context, IAntiforgery antiforgery, CreateCompetitionRequest request, ISettingsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        if (failure is not null)
            return failure;
        var result = await service.CreateCompetitionAsync(request, ct);
        return result.IsSuccess ? TypedResults.Created($"/api/settings/competitions/{result.Value.Id}", result.Value) : ToProblem(result.Error, context);
    }
    private static async Task<IResult> UpdateCompetitionAsync(Guid id, HttpContext context, IAntiforgery antiforgery, UpdateCompetitionRequest request, ISettingsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        if (failure is not null)
            return failure;
        return ToResult(await service.UpdateCompetitionAsync(id, request, ct), context);
    }
    private static async Task<IResult> ArchiveCompetitionAsync(Guid id, HttpContext context, IAntiforgery antiforgery, ISettingsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        if (failure is not null)
            return failure;
        return ToResult(await service.ArchiveCompetitionAsync(id, ct), context);
    }
    private static async Task<IResult> RestoreCompetitionAsync(Guid id, HttpContext context, IAntiforgery antiforgery, ISettingsService service, CancellationToken ct)
    {
        var failure = await AntiforgeryValidation.ValidateRequestAsync(context, antiforgery);
        if (failure is not null)
            return failure;
        return ToResult(await service.RestoreCompetitionAsync(id, ct), context);
    }
    private static IResult ToResult<T>(Result<T> result, HttpContext context) => result.IsSuccess ? TypedResults.Ok(result.Value) : ToProblem(result.Error, context);
    private static IResult ToProblem(Error error, HttpContext context)
    {
        var status = error.Code switch
        {
            "not_found" => StatusCodes.Status404NotFound,
            "duplicate_name" or "archived_record" => StatusCodes.Status409Conflict,
            "validation_failed" => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };
        return TypedResults.Problem(new ProblemDetails { Status = status, Title = "Request could not be completed", Detail = error.Message, Extensions = { ["code"] = error.Code, ["traceId"] = context.TraceIdentifier } });
    }
}
