using PlayerPerformance.Api.Configuration;

namespace PlayerPerformance.Api.Cors;

internal static class ApiCorsServiceCollectionExtensions
{
    public static IServiceCollection AddApiCors(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing"))
        {
            var deployment = configuration.GetSection(DeploymentOptions.SectionName).Get<DeploymentOptions>() ?? new DeploymentOptions();
            if (!deployment.IsSplitOrigin)
            {
                return services;
            }

            var origins = deployment.AllowedFrontendOrigins
                .Select(ProductionOptionsValidator.NormalizeOrigin)
                .Where(static origin => origin is not null)
                .Cast<string>()
                .ToArray();
            services.AddCors(options => options.AddPolicy(ApiCorsConstants.FrontendPolicyName, policy => policy
                .WithOrigins(origins)
                .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
                .WithHeaders("Content-Type", "Accept", Authentication.ApiAntiforgeryConstants.HeaderName)
                .AllowCredentials()));
            return services;
        }

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
