using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlayerPerformance.Infrastructure.Identity;

namespace PlayerPerformance.Api.Authentication;

internal sealed class PasswordChangeRequiredMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true && !CanBypassGate(context))
        {
            var user = await userManager.GetUserAsync(context.User);

            if (user?.RequiresPasswordChange == true)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                var problemDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();
                await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Title = "Password change required",
                        Detail = "You must change your password before accessing this resource.",
                        Extensions = { ["code"] = "password_change_required" }
                    }
                });

                return;
            }
        }

        await next(context);
    }

    private static bool CanBypassGate(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        return endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null
            || endpoint?.Metadata.GetMetadata<AllowPasswordChangeRequiredAttribute>() is not null;
    }
}
