using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using PlayerPerformance.Application.Settings;
using PlayerPerformance.Application.Teams;
using PlayerPerformance.Application.Players;

namespace PlayerPerformance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<SettingsService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<ITeamsService, TeamsService>();
        services.AddScoped<IPlayersService, PlayersService>();

        return services;
    }
}
