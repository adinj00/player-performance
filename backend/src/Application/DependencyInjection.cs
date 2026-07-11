using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using PlayerPerformance.Application.Settings;
using PlayerPerformance.Application.Teams;
using PlayerPerformance.Application.Players;
using PlayerPerformance.Application.Matches;

namespace PlayerPerformance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<SettingsService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<ITeamsService, TeamsService>();
        services.AddScoped<IPlayersService, PlayersService>();
        services.AddScoped<IPlayerTeamAssignmentsService, PlayerTeamAssignmentsService>();
        services.AddScoped<IMatchesService, MatchesService>();
        services.AddScoped<IMatchReportsService, MatchReportsService>();
        services.AddScoped<IMatchReportWorkflowGuard, MatchReportsService>();
        services.AddSingleton<IMatchStatisticsProfileService, MatchStatisticsProfileService>();
        services.AddScoped<IMatchStatisticsService, MatchStatisticsService>();

        return services;
    }
}
