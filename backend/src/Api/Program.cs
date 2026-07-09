using PlayerPerformance.Application;
using PlayerPerformance.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

app.MapGet("/health", () =>
    TypedResults.Ok(new HealthResponse(
        "ok",
        "PlayerPerformance.Api",
        DateTimeOffset.UtcNow)));

app.Run();

internal sealed record HealthResponse(
    string Status,
    string Service,
    DateTimeOffset TimestampUtc);
