using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PlayerPerformance.Api.ErrorHandling;

internal static class ProblemDetailsExtensions
{
    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                context.ProblemDetails.Status ??= context.HttpContext.Response.StatusCode;

                if (string.IsNullOrWhiteSpace(context.ProblemDetails.Title))
                {
                    context.ProblemDetails.Title = GetDefaultTitle(context.ProblemDetails.Status);
                }
            };
        });

        return services;
    }

    public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("PlayerPerformance.Api.ErrorHandling");

                if (exceptionFeature?.Error is not null)
                {
                    logger.LogError(
                        exceptionFeature.Error,
                        "Unhandled exception while processing request {Method} {Path}. TraceId: {TraceId}",
                        context.Request.Method,
                        context.Request.Path,
                        context.TraceIdentifier);
                }

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var problemDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();
                await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Unexpected server error"
                    }
                });
            });
        });

        return app;
    }

    public static IApplicationBuilder UseApiStatusCodeProblemDetails(this IApplicationBuilder app)
    {
        app.UseStatusCodePages(async statusCodeContext =>
        {
            var response = statusCodeContext.HttpContext.Response;
            if (response.HasStarted)
            {
                return;
            }

            if (!ShouldWriteProblemDetails(response.StatusCode))
            {
                return;
            }

            var problemDetailsService =
                statusCodeContext.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();

            await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = statusCodeContext.HttpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = response.StatusCode,
                    Title = GetDefaultTitle(response.StatusCode)
                }
            });
        });

        return app;
    }

    private static bool ShouldWriteProblemDetails(int statusCode)
    {
        return statusCode == StatusCodes.Status404NotFound
            || statusCode == StatusCodes.Status405MethodNotAllowed;
    }

    private static string GetDefaultTitle(int? statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status404NotFound => "Resource not found",
            StatusCodes.Status405MethodNotAllowed => "Method not allowed",
            StatusCodes.Status500InternalServerError => "Unexpected server error",
            _ => "Request could not be completed"
        };
    }
}
