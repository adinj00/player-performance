using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace PlayerPerformance.Api.Authentication;

internal static class AntiforgeryValidation
{
    public static async Task<IResult?> ValidateRequestAsync(
        HttpContext httpContext,
        IAntiforgery antiforgery)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(httpContext);
            return null;
        }
        catch (AntiforgeryValidationException)
        {
            return TypedResults.Problem(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid CSRF token",
                Detail = "A valid CSRF token is required for this request.",
                Extensions =
                {
                    ["traceId"] = httpContext.TraceIdentifier
                }
            });
        }
    }
}
