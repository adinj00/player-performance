using PlayerPerformance.Application;
using PlayerPerformance.Api.Authentication;
using PlayerPerformance.Infrastructure;
using PlayerPerformance.Api.Configuration;
using PlayerPerformance.Api.Cors;
using PlayerPerformance.Api.Endpoints;
using PlayerPerformance.Api.ErrorHandling;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddLocalDotEnvIfPresent(builder.Environment.ContentRootPath);
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
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiAuthentication(builder.Environment);
builder.Services.AddApiCors(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddApiProblemDetails();

var app = builder.Build();

app.UseApiExceptionHandling();
app.UseApiStatusCodeProblemDetails();
app.UseCors(ApiCorsConstants.FrontendPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapApiEndpoints();

app.Run();

public partial class Program;
