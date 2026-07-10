using PlayerPerformance.Api.Configuration;

namespace PlayerPerformance.Api.Cors;

internal static class ApiCorsServiceCollectionExtensions
{
    public static IServiceCollection AddApiCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var frontendOrigin = configuration[
            $"{PlayerPerformanceOptions.SectionName}:{nameof(PlayerPerformanceOptions.FrontendOrigin)}"];

        if (string.IsNullOrWhiteSpace(frontendOrigin))
        {
            return services;
        }

        services.AddCors(options =>
        {
            options.AddPolicy(ApiCorsConstants.FrontendPolicyName, policy =>
            {
                policy
                    .WithOrigins(frontendOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
