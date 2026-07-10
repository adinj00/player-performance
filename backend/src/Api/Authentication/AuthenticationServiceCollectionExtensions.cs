using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace PlayerPerformance.Api.Authentication;

internal static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddApiAuthentication(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        var useRelaxedSecurePolicy = environment.IsDevelopment() || environment.IsEnvironment("Testing");

        services.AddHttpContextAccessor();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            })
            .AddCookie(IdentityConstants.ApplicationScheme, options =>
            {
                options.Cookie.Name = "player-performance.auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.Path = "/";
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = useRelaxedSecurePolicy
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;
                options.Cookie.IsEssential = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context => WriteProblemDetailsResponseAsync(
                        context.HttpContext,
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized",
                        "Authentication is required to access this resource."),
                    OnRedirectToAccessDenied = context => WriteProblemDetailsResponseAsync(
                        context.HttpContext,
                        StatusCodes.Status403Forbidden,
                        "Forbidden",
                        "You do not have permission to access this resource.")
                };
            });

        services.AddAntiforgery(options =>
        {
            options.HeaderName = ApiAntiforgeryConstants.HeaderName;
            options.Cookie.Name = ApiAntiforgeryConstants.CookieName;
            options.Cookie.HttpOnly = false;
            options.Cookie.Path = "/";
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = useRelaxedSecurePolicy
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
            options.Cookie.IsEssential = true;
        });

        services.AddSingleton<IConfigureOptions<SecurityStampValidatorOptions>, ConfigureSecurityStampValidatorOptions>();

        return services;
    }

    private static async Task WriteProblemDetailsResponseAsync(
        HttpContext httpContext,
        int statusCode,
        string title,
        string detail)
    {
        httpContext.Response.StatusCode = statusCode;

        if (httpContext.Response.HasStarted)
        {
            return;
        }

        var problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();

        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            }
        });
    }

    private sealed class ConfigureSecurityStampValidatorOptions : IConfigureOptions<SecurityStampValidatorOptions>
    {
        public void Configure(SecurityStampValidatorOptions options)
        {
            options.ValidationInterval = TimeSpan.FromMinutes(5);
        }
    }
}
