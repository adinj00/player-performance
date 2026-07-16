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
using PlayerPerformance.Application.Matches;
using PlayerPerformance.Infrastructure.Matches;
using PlayerPerformance.Application.Auditing;
using PlayerPerformance.Infrastructure.Auditing;
using PlayerPerformance.Application.Files;
using PlayerPerformance.Infrastructure.Files;
using PlayerPerformance.Application.Media;
using PlayerPerformance.Infrastructure.Media;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PlayerPerformance.Application.Imports;
using PlayerPerformance.Infrastructure.Imports;

namespace PlayerPerformance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services.AddInfrastructure(configuration, new DevelopmentHostEnvironment());

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = GetRequiredDefaultConnectionString(configuration);

        services.AddSingleton<ISystemClock, SystemClock>();

        services.AddSingleton<IValidateOptions<FileStorageOptions>, FileStorageOptionsValidator>();
        services
            .AddOptions<FileStorageOptions>()
            .Bind(configuration.GetSection(FileStorageOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddSingleton<IValidateOptions<MediaOptions>, MediaOptionsValidator>();
        services.AddOptions<MediaOptions>().Bind(configuration.GetSection(MediaOptions.SectionName)).ValidateOnStart();
        services.AddSingleton<IValidateOptions<ImportOptions>, ImportOptionsValidator>();
        services.AddOptions<ImportOptions>().Bind(configuration.GetSection(ImportOptions.SectionName)).ValidateOnStart();

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
        services.AddScoped<IPlayerTeamAssignmentsRepository, PlayerTeamAssignmentsRepository>();
        services.AddScoped<IMatchesRepository, MatchesRepository>();
        services.AddScoped<IMatchLineupRepository, MatchLineupRepository>();
        services.AddScoped<IMatchReportsRepository, MatchReportsRepository>();
        services.AddScoped<IMatchStatisticsRepository, MatchStatisticsRepository>();
        services.AddScoped<IMatchStatisticsCleanup, MatchStatisticsRepository>();
        services.AddScoped<AuditStore>();
        services.AddScoped<IAuditWriter>(provider => provider.GetRequiredService<AuditStore>());
        services.AddScoped<IAuditHistoryRepository>(provider => provider.GetRequiredService<AuditStore>());
        services.AddScoped<IStaffUsersService, IdentityStaffUsersService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddSingleton<IImportProcessorRegistry, ImportProcessorRegistry>();
        services.AddScoped<IImportService, ImportService>();
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

    private sealed class DevelopmentHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "PlayerPerformance";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
