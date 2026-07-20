namespace PlayerPerformance.Api.Configuration;

internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(static state =>
        {
            var response = ((HttpContext)state).Response;
            response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
            response.Headers.TryAdd("X-Frame-Options", "DENY");
            response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
            response.Headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
            response.Headers.TryAdd("Content-Security-Policy", "default-src 'self'; base-uri 'self'; frame-ancestors 'none'; form-action 'self'; object-src 'none'");
            response.Headers.TryAdd("Cache-Control", "no-store");
            return Task.CompletedTask;
        }, context);
        await next(context);
    }
}
