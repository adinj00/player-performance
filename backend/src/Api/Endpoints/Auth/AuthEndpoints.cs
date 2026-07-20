using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using PlayerPerformance.Api.Authentication;
using PlayerPerformance.Api.Configuration;
using PlayerPerformance.Application.Auth;
using PlayerPerformance.Domain.Common.Errors;

namespace PlayerPerformance.Api.Endpoints;

internal static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Auth");

        group.MapGet("/csrf", (HttpContext httpContext, IAntiforgery antiforgery) =>
            {
                var tokens = antiforgery.GetAndStoreTokens(httpContext);
                return TypedResults.Ok(new CsrfTokenResponse(ApiAntiforgeryConstants.HeaderName, tokens.RequestToken));
            })
            .AllowAnonymous();

        group.MapGet("/session", GetSessionAsync)
            .AllowAnonymous();

        group.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .RequireRateLimiting(ProductionServiceCollectionExtensions.AuthSensitivePolicy);

        group.MapPost("/change-password", ChangePasswordAsync)
            .RequireAuthorization()
            .WithMetadata(new AllowPasswordChangeRequiredAttribute());

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithMetadata(new AllowPasswordChangeRequiredAttribute())
            .RequireRateLimiting(ProductionServiceCollectionExtensions.TokenSetupPolicy);

        return endpoints;
    }

    private static async Task<IResult> GetSessionAsync(
        IAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await authenticationService.GetCurrentSessionAsync(cancellationToken));
    }

    private static async Task<IResult> LoginAsync(
        HttpContext httpContext,
        IAntiforgery antiforgery,
        LoginRequest request,
        IAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        var antiforgeryFailure = await AntiforgeryValidation.ValidateRequestAsync(httpContext, antiforgery);
        if (antiforgeryFailure is not null)
        {
            return antiforgeryFailure;
        }

        var result = await authenticationService.LoginAsync(request, cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : ToProblem(result.Error, httpContext);
    }

    private static async Task<IResult> ChangePasswordAsync(
        HttpContext httpContext,
        IAntiforgery antiforgery,
        ChangePasswordRequest request,
        IAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        var antiforgeryFailure = await AntiforgeryValidation.ValidateRequestAsync(httpContext, antiforgery);
        if (antiforgeryFailure is not null)
        {
            return antiforgeryFailure;
        }

        var result = await authenticationService.ChangePasswordAsync(request, cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : ToProblem(result.Error, httpContext);
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext httpContext,
        IAntiforgery antiforgery,
        IAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        var antiforgeryFailure = await AntiforgeryValidation.ValidateRequestAsync(httpContext, antiforgery);
        if (antiforgeryFailure is not null)
        {
            return antiforgeryFailure;
        }

        await authenticationService.LogoutAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    private static IResult ToProblem(Error error, HttpContext context)
    {
        var status = error.Code switch
        {
            "invalid_credentials" => StatusCodes.Status401Unauthorized,
            "account_unavailable" => StatusCodes.Status403Forbidden,
            "invalid_request" or "password_confirmation_mismatch" => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status422UnprocessableEntity
        };

        return TypedResults.Problem(new ProblemDetails
        {
            Status = status,
            Title = status == StatusCodes.Status401Unauthorized ? "Unauthorized" : "Request could not be completed",
            Detail = error.Message,
            Extensions = { ["code"] = error.Code, ["traceId"] = context.TraceIdentifier }
        });
    }

    private sealed record CsrfTokenResponse(string CsrfTokenHeaderName, string? RequestToken);
}
