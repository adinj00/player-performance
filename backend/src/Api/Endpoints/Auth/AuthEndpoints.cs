using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using PlayerPerformance.Api.Authentication;
using PlayerPerformance.Domain.Users;
using PlayerPerformance.Infrastructure.Identity;

namespace PlayerPerformance.Api.Endpoints;

internal static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapGet("/csrf", (HttpContext httpContext, IAntiforgery antiforgery) =>
            {
                antiforgery.GetAndStoreTokens(httpContext);

                return TypedResults.Ok(new CsrfTokenResponse(ApiAntiforgeryConstants.HeaderName));
            })
            .AllowAnonymous();

        group.MapGet("/session", GetSessionAsync)
            .AllowAnonymous();

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> GetSessionAsync(
        HttpContext httpContext,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        if (httpContext.User.Identity?.IsAuthenticated != true)
        {
            return TypedResults.Ok(SessionResponse.Unauthenticated());
        }

        var user = await userManager.GetUserAsync(httpContext.User);

        if (user is null || await IsBlockedSessionAsync(userManager, user))
        {
            await signInManager.SignOutAsync();
            return TypedResults.Ok(SessionResponse.Unauthenticated());
        }

        return TypedResults.Ok(SessionResponse.Authenticated(new SessionUserResponse(
            user.Id.ToString(),
            user.Email ?? string.Empty,
            user.AccountStatus.ToString(),
            user.RequiresPasswordChange)));
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext httpContext,
        IAntiforgery antiforgery,
        SignInManager<ApplicationUser> signInManager)
    {
        var antiforgeryFailure = await AntiforgeryValidation.ValidateRequestAsync(httpContext, antiforgery);

        if (antiforgeryFailure is not null)
        {
            return antiforgeryFailure;
        }

        await signInManager.SignOutAsync();
        return TypedResults.NoContent();
    }

    private static async Task<bool> IsBlockedSessionAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user)
    {
        return user.AccountStatus is UserAccountStatus.DISABLED or UserAccountStatus.LOCKED
            || await userManager.IsLockedOutAsync(user);
    }

    private sealed record CsrfTokenResponse(string CsrfTokenHeaderName);

    private sealed record SessionResponse(bool IsAuthenticated, SessionUserResponse? User)
    {
        public static SessionResponse Unauthenticated() => new(false, null);

        public static SessionResponse Authenticated(SessionUserResponse user) => new(true, user);
    }

    private sealed record SessionUserResponse(
        string Id,
        string Email,
        string AccountStatus,
        bool MustChangePassword);
}
