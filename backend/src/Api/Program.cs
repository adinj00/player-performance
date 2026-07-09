using PlayerPerformance.Application;
using PlayerPerformance.Infrastructure;
using PlayerPerformance.Api.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddLocalDotEnvIfPresent(builder.Environment.ContentRootPath);

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
builder.Services.AddInfrastructure();

var app = builder.Build();

app.MapGet("/health", (IOptions<PlayerPerformanceOptions> options) =>
    TypedResults.Ok(new HealthResponse(
        "ok",
        options.Value.ServiceName,
        DateTimeOffset.UtcNow)));

app.Run();

internal sealed record HealthResponse(
    string Status,
    string Service,
    DateTimeOffset TimestampUtc);
