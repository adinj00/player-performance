using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlayerPerformance.Application.Abstractions.Time;
using PlayerPerformance.Application.Auth;
using PlayerPerformance.Application.Authorization;
using PlayerPerformance.Infrastructure.Authorization;
using PlayerPerformance.Infrastructure.Identity;
using PlayerPerformance.Infrastructure.Persistence;
using PlayerPerformance.Infrastructure.Time;
using PlayerPerformance.Application.Settings;
using PlayerPerformance.Infrastructure.Settings;
using PlayerPerformance.Application.Teams;
using PlayerPerformance.Infrastructure.Teams;
using PlayerPerformance.Application.Users;
using PlayerPerformance.Infrastructure.Staff;
using PlayerPerformance.Application.Players;
using PlayerPerformance.Infrastructure.Players;

namespace PlayerPerformance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = GetRequiredDefaultConnectionString(configuration);

        services.AddSingleton<ISystemClock, SystemClock>();

        services
            .AddOptions<FirstAdminBootstrapOptions>()
            .Bind(configuration.GetSection(FirstAdminBootstrapOptions.SectionName))
            .ValidateOnStart();

        services.AddScoped<FirstAdminBootstrapper>();
        services.AddScoped<FirstAdminRoleHandoff>();
        services.AddScoped<ICurrentUserAccess, CurrentUserAccessResolver>();
        services.AddScoped<ITeamAccessService, TeamAccessService>();
        services.AddScoped<IAuthenticationService, IdentityAuthenticationService>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<ITeamsRepository, TeamsRepository>();
        services.AddScoped<IPlayersRepository, PlayersRepository>();
        services.AddScoped<IStaffUsersService, IdentityStaffUsersService>();
        services.AddScoped<TeamStartupInitializer>();

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 12;
                options.Password.RequiredUniqueChars = 4;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;

                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddSignInManager<ApplicationSignInManager>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services
            .AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(
                name: "database",
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                tags: ["ready"]);

        return services;
    }

    private static string GetRequiredDefaultConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string 'ConnectionStrings:DefaultConnection' is required.");
        }

        return connectionString;
    }
}
