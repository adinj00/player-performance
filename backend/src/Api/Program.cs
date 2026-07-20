using PlayerPerformance.Application;
using PlayerPerformance.Api.Authentication;
using PlayerPerformance.Infrastructure;
using PlayerPerformance.Api.Configuration;
using PlayerPerformance.Api.Cors;
using PlayerPerformance.Api.Endpoints;
using PlayerPerformance.Api.ErrorHandling;
using PlayerPerformance.Infrastructure.Identity;
using PlayerPerformance.Api.Authorization;
using PlayerPerformance.Infrastructure.Teams;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddLocalDotEnvIfPresent(builder.Environment.ContentRootPath, builder.Environment);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

builder.Services
    .AddOptions<PlayerPerformanceOptions>()
    .Bind(builder.Configuration.GetSection(PlayerPerformanceOptions.SectionName))
    .Validate(
        static options => !string.IsNullOrWhiteSpace(options.ServiceName),
        $"{PlayerPerformanceOptions.SectionName}:{nameof(PlayerPerformanceOptions.ServiceName)} is required.")
    .Validate(
        static options => PlayerPerformanceOptions.IsValidFrontendOrigin(options.FrontendOrigin),
        $"{PlayerPerformanceOptions.SectionName}:{nameof(PlayerPerformanceOptions.FrontendOrigin)} must be an absolute HTTP or HTTPS URL when provided.")
    .ValidateOnStart();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddProductionConfiguration(builder.Configuration, builder.Environment);
builder.Services.AddApiAuthentication(builder.Environment);
builder.Services.AddApiCors(builder.Configuration, builder.Environment);
builder.Services.AddStaffAuthorization();
builder.Services.AddApiProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

var deployment = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<DeploymentOptions>>().Value;
app.UseForwardedHeaders(ProductionServiceCollectionExtensions.CreateForwardedHeadersOptions(deployment));
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

await app.Services.BootstrapFirstAdminAsync(app.Environment);
await app.Services.InitializeTeamsAsync(app.Environment);

app.UseApiExceptionHandling();
app.UseApiStatusCodeProblemDetails();
app.UseMiddleware<SecurityHeadersMiddleware>();
if (deployment.IsSplitOrigin || app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing")) app.UseCors(ApiCorsConstants.FrontendPolicyName);
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<PasswordChangeRequiredMiddleware>();
app.UseAuthorization();

app.MapApiEndpoints(app.Environment);

app.Run();

public partial class Program;
