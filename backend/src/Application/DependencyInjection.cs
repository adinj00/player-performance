using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using PlayerPerformance.Application.Settings;

namespace PlayerPerformance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<SettingsService>();
        services.AddScoped<ISettingsService, SettingsService>();
        
        return services;
    }
}
